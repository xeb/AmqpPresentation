using System.Net;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Client
{
    public class PerfTestWebPostClient : IRandomTest
    {
        private readonly string _url;

        public PerfTestWebPostClient(string url)
        {
            _url = url;
        }

        public SomeObject Random(SomeObject request)
        {
            var webclient = new WebClient();
            var value = webclient.UploadString(_url, "POST", request.Serialize());
            return value.Deserialize<SomeObject>();
        }

        public string Name
        {
            get { return GetType().FullName + "::" + _url; }
        }

    }
}