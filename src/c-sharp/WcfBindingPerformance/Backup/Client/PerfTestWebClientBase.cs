using System.ServiceModel;
using System.ServiceModel.Description;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Client
{
    public class PerfTestWebClientBase : ClientBase<IPerfTestWeb>, IPerfTestWeb, IRandomTest
    {
        public PerfTestWebClientBase(string config) : base(config)
        {
            ChannelFactory.Endpoint.Behaviors.Add(new WebHttpBehavior());
        }

        public PerfTestWebClientBase() : this("PerfTestWeb") { }

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