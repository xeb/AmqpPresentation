using System;
using System.IO;
using System.Threading;
using System.Web;
using WcfBindingPerformance.Models;

namespace WcfBindingPerformance.Services
{
    /// <summary>
    /// Summary description for Handler
    /// </summary>
    public class Handler : IHttpAsyncHandler
    {
        public void ProcessRequest(HttpContext context)
        {

        }

        public bool IsReusable
        {
            get { return false; }
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var asynch = new AsynchOperation(cb, context, extraData);
            asynch.StartAsyncWork();
            return asynch;
        }

        public void EndProcessRequest(IAsyncResult result)
        {
        }


        private class AsynchOperation : IAsyncResult
        {
            private bool _completed;
            private readonly Object _state;
            private readonly AsyncCallback _callback;
            private readonly HttpContext _context;

            bool IAsyncResult.IsCompleted { get { return _completed; } }
            WaitHandle IAsyncResult.AsyncWaitHandle { get { return null; } }
            Object IAsyncResult.AsyncState { get { return _state; } }
            bool IAsyncResult.CompletedSynchronously { get { return false; } }

            public AsynchOperation(AsyncCallback callback, HttpContext context, Object state)
            {
                _callback = callback;
                _context = context;
                _state = state;
                _completed = false;
            }

            public void StartAsyncWork()
            {
                ThreadPool.QueueUserWorkItem(StartAsyncTask, null);
            }

            private void StartAsyncTask(Object workItemState)
            {
                string request;
                using (var sr = new StreamReader(_context.Request.InputStream))
                    request = sr.ReadToEnd();

                try
                {
                    var obj = !string.IsNullOrWhiteSpace(request) ? request.Deserialize<SomeObject>() : new SomeObject();

                    ArbitraryCalculation.SortList();

                    _context.Response.Write(new SomeObject { Id = obj.Id, Name = obj.Name }.Serialize());
                    _context.Response.ContentType = "application/json";
                }
                catch (Exception ex)
                {
                    _context.Response.Write(ex.Message + "\r\n\r\n");
                    _context.Response.Write(ex.StackTrace + "\r\n");
                    _context.Response.Write("Request was:\r\n\r\n-----------------------\r\n");
                    _context.Response.Write(request);
                    _context.Response.ContentType = "text/plain";
                }
                _completed = true;
                _callback(this);
            }
        }
    }
}