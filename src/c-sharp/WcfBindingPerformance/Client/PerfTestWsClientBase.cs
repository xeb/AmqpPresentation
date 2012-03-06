using System.ServiceModel;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Client
{
    public class PerfTestWsClientBase : ClientBase<IPerfTestWs>, IPerfTestWs, IRandomTest
    {
        public PerfTestWsClientBase(string config)
            : base(config)
        {
        }

        public PerfTestWsClientBase() : this("PerfTestWs") { }

        public SomeObject Random(SomeObject request)
        {
            return Channel.Random(request);
        }

        public string Name
        {
            get { return GetType().FullName; }
        }

    }
}