using Nancy;
using Nancy.ModelBinding;
using WcfBindingPerformance.Models;
using WcfBindingPerformance.Services;

namespace WcfBindingPerformance.nancy
{
    public class PerfTestNancy : NancyModule
    {
        public PerfTestNancy()
        {
            Get["/nancy/"] = Random;
            Post["/nancy/"] = Random;
        }

        private Response Random(dynamic arg)
        {
            var request = this.Bind<SomeObject>();

            ArbitraryCalculation.SortList();

            return Response.AsJson(new SomeObject { Id = request.Id, Name = request.Name });
        }
    }
}