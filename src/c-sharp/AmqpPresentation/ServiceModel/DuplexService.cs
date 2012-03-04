using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Timers;

namespace AmqpPresentation.ServiceModel
{
    [ServiceContract(CallbackContract = typeof(IMeetAndGreetCallback))]
    public interface IMeetAndGreetService
    {
        [OperationContract(IsOneWay = true)]
        void ExchangeInfo(Introduction intro);
    }

    public interface IMeetAndGreetCallback
    {
        [OperationContract(IsOneWay = true)]
        void CallupOneday(Introduction intro);
    }

    [DataContract]
    public class Introduction
    {
        public static Introduction Create(string name = "")
        {
            return new Introduction
            {
                Name = string.IsNullOrWhiteSpace(name) ? Environment.MachineName : name,
                Timestamp = DateTime.Now,
            };
        }

        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public DateTime Timestamp { get; set; }
    }

    public class MeetAndGreetService : IMeetAndGreetService
    {
        public void ExchangeInfo(Introduction intro)
        {
            Console.WriteLine("Received intro from {0} on {1}", intro.Name, intro.Timestamp);
            var callbackChannel = OperationContext.Current.GetCallbackChannel<IMeetAndGreetCallback>();

            Thread.Sleep(500);

            callbackChannel.CallupOneday(Introduction.Create("Dad"));
        }
    }

    public class MeetAndGreetClient : IMeetAndGreetService
    {
        private readonly DuplexChannelFactory<IMeetAndGreetService> _channelFactory;
        public MeetAndGreetClient()
        {
            var binding = new RabbitMQ.ServiceModel.RabbitMQBinding(Program.Broker, 5672);
            var instanceContext = new InstanceContext(new IfSomeoneCallsMe());
            _channelFactory = new DuplexChannelFactory<IMeetAndGreetService>(instanceContext, binding, new EndpointAddress(Program.Protocol + ":///MeetAndGreet"));
        }

        public void ExchangeInfo(Introduction echo)
        {
            var channel = _channelFactory.CreateChannel();
            channel.ExchangeInfo(echo);
        }
    }

    public class IfSomeoneCallsMe : IMeetAndGreetCallback
    {
        public void CallupOneday(Introduction intro)
        {
            Console.WriteLine("OMG!  Its you, '{0}' at {1}", intro.Name, intro.Timestamp);
        }
    }

    public class MeetAndGreetServiceHostStarter
    {
        private readonly ServiceHost _host;
        private static readonly System.Timers.Timer Ticker = new System.Timers.Timer(2000);

        public MeetAndGreetServiceHostStarter()
        {
            _host = new ServiceHost(typeof(MeetAndGreetService), new Uri(Program.Protocol + ":///"));
            Ticker.Enabled = true;
            Ticker.Elapsed += Tick;
        }

        public void Start()
        {
            var binding = new RabbitMQ.ServiceModel.RabbitMQBinding(Program.Broker, 5672);

            _host.AddServiceEndpoint(typeof(IMeetAndGreetService), binding, "MeetAndGreet");
            _host.Open();
        }

        public void Stop()
        {
            _host.Close();
        }

        private static void Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                var client = new MeetAndGreetClient();
                Console.WriteLine("\r\nSending an Intro!");
                client.ExchangeInfo(Introduction.Create());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
