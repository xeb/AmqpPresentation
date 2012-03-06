using System.ServiceModel;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Client
{
    public class PerfTestTcpClientBase : ClientBase<IPerfTestTcp>, IPerfTestTcp, IRandomTest
    {
        public PerfTestTcpClientBase(string config) : base(config) { }
        public PerfTestTcpClientBase() : this("PerfTestTcp") { }

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