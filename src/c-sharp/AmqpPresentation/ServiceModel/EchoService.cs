﻿using System;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Timers;

namespace AmqpPresentation.ServiceModel.Contracts
{
    [ServiceContract]
    public interface IEchoService
    {
        [OperationContract]
        Echo SendEcho(Echo echo);
    }

    [DataContract]
    public class Echo
    {
        public static Echo Create()
        {
            return new Echo { Sender = Environment.MachineName, Timestamp = DateTime.Now };
        }

        [DataMember]
        public string Sender { get; set;}
        
        [DataMember]
        public DateTime Timestamp { get; set;}
    }

    public class EchoService : IEchoService
    {
        public Echo SendEcho(Echo echo)
        {
            Console.WriteLine("Received Echo from {0} at {1} (its now {2})", echo.Sender, echo.Timestamp, DateTime.Now);
            return Echo.Create();
        }
    }

    public class EchoServiceClient : IEchoService
    {
        private readonly ChannelFactory<IEchoService> _channelFactory;
        public EchoServiceClient()
        {
            var binding = new RabbitMQ.ServiceModel.RabbitMQBinding("localhost", 5672);
            _channelFactory = new ChannelFactory<IEchoService>(binding, new EndpointAddress("soap.amqp:///Echo"));
        }

        public Echo SendEcho(Echo echo)
        {
            var channel = _channelFactory.CreateChannel();
            return channel.SendEcho(echo);
        }
    }

    public class EchoServiceHostStarter
    {
        private readonly ServiceHost _host;
        private static readonly Timer EchoTimer = new Timer(5000);

        public EchoServiceHostStarter()
        {
            _host = new ServiceHost(typeof(EchoService), new Uri("soap.amqp:///"));
            EchoTimer.Enabled = true;
            EchoTimer.Elapsed += Tick;
        }

        private static void Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                var client = new EchoServiceClient();

                client.SendEcho(Echo.Create());

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void Start()
        {
            var binding = new RabbitMQ.ServiceModel.RabbitMQBinding("localhost", 5672);

            _host.AddServiceEndpoint(typeof(IEchoService), binding, "Echo");
            _host.Open();
        }

        public void Stop()
        {
            _host.Close();
        }
    }
}
