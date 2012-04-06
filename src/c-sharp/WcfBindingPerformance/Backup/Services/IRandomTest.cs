using WcfBindingPerformance.Models;

namespace WcfBindingPerformance.Services
{
    public interface IRandomTest
    {
        string Name { get; }
        SomeObject Random(SomeObject request);
    }
}
