using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;

namespace WcfBindingPerformance.Services
{
    [ServiceContract(CallbackContract = typeof(IDuplexCallback))]
    public interface IDuplexService
    {
        [OperationContract(IsOneWay = true)]
        void GetDate(GetDateRequest request);
    }

    public interface IDuplexCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnReceiveDate(GetDateResponse response);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = false)]
    public class DuplexTcpService : DuplexService
    {
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = true)]
    public class DuplexWsService : DuplexService
    {
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, UseSynchronizationContext = true)] // strange things happen when this is set to false (in ASP.NET, see error.txt)
    public class DuplexRabbitService : DuplexService
    {
    }

    public abstract class DuplexService : IDuplexService
    {
        public void GetDate(GetDateRequest request)
        {
            Debug.WriteLine("[Server] {0}", request.RequestTime);
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IDuplexCallback>();

            ArbitraryCalculation.SortList();

            callbackChannel.OnReceiveDate(new GetDateResponse { ResponseTime = DateTime.Now });
        }
    }

    public class DuplexClient: IDuplexCallback, IDuplexService
    {
        private readonly DuplexChannelFactory<IDuplexService> _channelFactory;

        public DuplexClient(Binding binding, string address)
        {
            _channelFactory = new DuplexChannelFactory<IDuplexService>(this, binding, new EndpointAddress(address));
        }

        public void OnReceiveDate(GetDateResponse response)
        {
            Debug.WriteLine("[Client] {0}", response.ResponseTime);
        }

        public void GetDate(GetDateRequest request)
        {
            var channel = _channelFactory.CreateChannel();
            channel.GetDate(new GetDateRequest { RequestTime = DateTime.Now });
        }
    }

    public class DuplexTcpClient : DuplexClient
    {
        public DuplexTcpClient(string address)
            : base(new NetTcpBinding { Security = { Mode = SecurityMode.None } }, address)
        {

        }
    }

    public class DuplexWsClient : DuplexClient
    {
        public DuplexWsClient(string address)
            : base(new WSDualHttpBinding { Security = { Mode = WSDualHttpSecurityMode.None } }, address)
        {

        }
    }

    //public class DuplexRabbitClient : DuplexClient
    //{
    //    public DuplexRabbitClient(string address)
    //        : base(new RabbitMQ.ServiceModel.RabbitMQBinding("localhost", 5672), address)
    //    {

    //    }
    //}

    public class Callback : IDuplexCallback
    {
        private readonly bool _writeToConsole;
        public Callback(bool writeToConsole)
        {
            _writeToConsole = writeToConsole;
        }

        public void OnReceiveDate(GetDateResponse response)
        {
            if(_writeToConsole)
            {
                Console.WriteLine(response.ResponseTime);
            }
        }
    }

    public class DuplexRabbitClient : DuplexClientBase<IDuplexService>, IDuplexService
    {
        public DuplexRabbitClient(bool writeToConsole)
            : base(new InstanceContext(new Callback(writeToConsole)), "DuplexRabbit")
        {
        }

        public void GetDate(GetDateRequest request)
        {
            Channel.GetDate(request);
        }
    }
}