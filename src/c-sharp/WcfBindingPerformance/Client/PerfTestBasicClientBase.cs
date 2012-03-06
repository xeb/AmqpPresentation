using System;
using System.ServiceModel;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.Client
{
    public class PerfTestBasicClientBase : ClientBase<IPerfTestBasic>, IPerfTestBasic, IRandomTest
    {

        public PerfTestBasicClientBase(string config)
            : base(config)
        {
        }

        public PerfTestBasicClientBase() : this("PerfTestBasic") { }

        public string Name
        {
            get { return GetType().FullName; }
        }

        public SomeObject Random(SomeObject request)
        {
            return Channel.Random(request);
        }
    }
}