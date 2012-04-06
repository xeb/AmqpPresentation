using System.ServiceModel;
using System.ServiceModel.Web;
using WcfBindingPerformance.Models;

namespace WcfBindingPerformance.Services
{
    #region NetTcp
    
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerfTestTcp : IPerfTestTcp
    {
        public SomeObject Random(SomeObject request)
        {
            return new SomeObject { ElapsedTime = ArbitraryCalculation.SortList() };
        }
    }

    [ServiceContract]
    public interface IPerfTestTcp
    {
        [OperationContract]
        SomeObject Random(SomeObject request);
    }

    #endregion

    #region Web

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerfTestWeb : IPerfTestWeb
    {
        public SomeObject Random(SomeObject request)
        {
            return new SomeObject { ElapsedTime = ArbitraryCalculation.SortList() };
        }
    }

    [ServiceContract]
    public interface IPerfTestWeb
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "Random", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped)]
        SomeObject Random(SomeObject request);
    }

    #endregion

    #region Basic Http

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerfTestBasic : IPerfTestBasic
    {
        public SomeObject Random(SomeObject request)
        {
            return new SomeObject { ElapsedTime = ArbitraryCalculation.SortList() };
        }
    }

    [ServiceContract]
    public interface IPerfTestBasic
    {
        [OperationContract]
        SomeObject Random(SomeObject request);
    }

    #endregion

    #region WS Http

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerfTestWs : IPerfTestWs
    {
        public SomeObject Random(SomeObject request)
        {
            return new SomeObject { ElapsedTime = ArbitraryCalculation.SortList() };
        }
    }

    [ServiceContract]
    public interface IPerfTestWs
    {
        [OperationContract]
        SomeObject Random(SomeObject request);
    }

    #endregion

    #region RabbitMQ

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class PerfTestRabbit : IPerfTestRabbit
    {
        public SomeObject Random(SomeObject request)
        {
            return new SomeObject { ElapsedTime = ArbitraryCalculation.SortList() };
        }
    }

    [ServiceContract]
    public interface IPerfTestRabbit
    {
        [OperationContract]
        SomeObject Random(SomeObject request);
    }

    #endregion
}