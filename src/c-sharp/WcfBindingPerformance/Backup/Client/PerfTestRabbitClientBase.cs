using System.ServiceModel;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Client
{
    public class PerfTestRabbitClientBase : ClientBase<IPerfTestRabbit>, IPerfTestRabbit, IRandomTest
    {
        public PerfTestRabbitClientBase(string config)
            : base(config)
        {
        }

        public PerfTestRabbitClientBase() : this("PerfTestRabbit") { }

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