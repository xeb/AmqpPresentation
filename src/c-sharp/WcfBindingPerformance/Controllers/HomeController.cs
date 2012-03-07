using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WcfBindingPerformance.Client;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Controllers
{
    public class HomeController : Controller
    {
        private const int _numberThreads = 1;
        private const int _iterations = 1;
        private const int _batches = 1;

        private const string _nancyUrl = "http://wcfbindingtest:1337/WcfBindingPerformance/nancy/";
        private const string _handlerUrl = "http://wcfbindingtest:1337/WcfBindingPerformance/services/handler.ashx";

        public ActionResult Index()
        {
            return View();
        }

        #region Random Perf Tests
        
        public ActionResult All()
        {
            var content = RunPerftest(new PerfTestTcpClientBase()) 
                          + RunPerftest(new PerfTestWebClientBase()) 
                          + RunPerftest(new PerfTestBasicClientBase())
                          + RunPerftest(new PerfTestWsClientBase())
                          + RunPerftest(new PerfTestRabbitClientBase())
                          + RunPerftest(new PerfTestWebPostClient(_nancyUrl))
                          + RunPerftest(new PerfTestWebPostClient(_handlerUrl))
                          ;

            return Content(content, "text/plain");
        }

        public ActionResult PerfTestTcp()
        {
            return Perftest(new PerfTestTcpClientBase());
        }

        public ActionResult PerfTestWeb()
        {
            return Perftest(new PerfTestWebClientBase());
        }

        public ActionResult PerfTestBasic()
        {
            return Perftest(new PerfTestBasicClientBase());
        }

        public ActionResult PerfTestWs()
        {
            return Perftest(new PerfTestWsClientBase());
        }

        public ActionResult PerfTestRabbit()
        {
            return Perftest(new PerfTestRabbitClientBase());
        }

        public ActionResult PerfTestNancy()
        {
            return Perftest(new PerfTestWebPostClient(_nancyUrl));
        }

        public ActionResult PerfTestHandler()
        {
            return Perftest(new PerfTestWebPostClient(_handlerUrl));
        }
        
        #endregion

        #region Duplex Tests

        public ActionResult DuplexTcp()
        {
            return Perftest(new DuplexTcpClient("net.tcp://localhost:1338/WcfBindingPerformance/Services/DuplexTcp.svc"));
        }

        public ActionResult DuplexWs()
        {
            return Perftest(new DuplexWsClient("http://localhost:1337/WcfBindingPerformance/Services/DuplexWs.svc"));
        }

        public ActionResult DuplexRabbit()
        {
            return Perftest(new DuplexRabbitClient(false));
        }

        #endregion

        #region Private - Random Perf Tests

        private ActionResult Perftest(IRandomTest svc)
        {
            return Content(RunPerftest(svc), "text/plain");
        }

        private static string RunPerftest(IRandomTest svc)
        {
            var sw = new Stopwatch();
            sw.Start();

            var results = new List<double>();

            for (int j = 0; j < _iterations; j++)
            {
                var isw = new Stopwatch();
                isw.Start();

                for (int l = 0; l < _batches; l++)
                {
                    var tasks = new List<Task>();

                    for (int i = 0; i < _numberThreads; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            svc.Random(new SomeObject());
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }

                isw.Stop();
                results.Add(isw.ElapsedMilliseconds);
            }

            sw.Stop();

            var content = svc.GetType().FullName + "\r\n" +
                svc.Name + "\r\n" +
                string.Join("\r\n", results) +
                "\r\n\r\nTotal:\t\t" + sw.ElapsedMilliseconds +
                "\r\n\r\nAverage:\t" + results.Average() +
                "\r\n\r\n";

            return content;
        }

        #endregion

        #region Private - Duplex Tests
        
        private ActionResult Perftest(IDuplexService svc)
        {
            var sw = new Stopwatch();
            sw.Start();
            var content = RunPerftest(svc);
            sw.Stop();

            content += "\r\n\r\n" + sw.ElapsedMilliseconds + "ms TOTAL";

            return Content(content, "text/plain");
        }

        private static string RunPerftest(IDuplexService svc)
        {
            var sw = new Stopwatch();
            sw.Start();

            var results = new List<double>();
            var errors = new List<Exception>();

            for (int j = 0; j < _iterations; j++)
            {
                var isw = new Stopwatch();
                isw.Start();

                for (int l = 0; l < _batches; l++)
                {
                    var tasks = new List<Task>();
                    
                    for (int i = 0; i < _numberThreads; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                svc.GetDate(new GetDateRequest { RequestTime = DateTime.Now });
                            }
                            catch (Exception ex)
                            {
                                lock(errors)
                                {
                                    errors.Add(ex);
                                }
                            }
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }

                isw.Stop();
                results.Add(isw.ElapsedMilliseconds);
            }

            sw.Stop();

            var content = svc.GetType().FullName + "\r\n" +
                string.Join("\r\n", results) +
                "\r\n\r\nTotal:\t\t" + sw.ElapsedMilliseconds +
                "\r\n\r\nAverage:\t" + results.Average() +
                "\r\n\r\nErrors:\t" + errors.Count() +
                "\r\n\r\n";

            return content;
        }

        #endregion
    }
}
