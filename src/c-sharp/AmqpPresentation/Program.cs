﻿using System;
using System.Threading;
using AmqpPresentation.ServiceModel;
using RabbitMQ.Client;

namespace AmqpPresentation
{
    public class Program
    {
        public const string Broker = "localhost";

        #region Protocol for WCF
        public const string Protocol = "soap.amqp";
        #endregion

        public static void Main(string[] args)
        {
            Console.WriteLine("Connecting to {0}...", Broker);
            var connectionFactory = new ConnectionFactory
            {
                Endpoint = new AmqpTcpEndpoint(Broker),
                Protocol = Protocols.FromEnvironment(),
                Port = AmqpTcpEndpoint.UseDefaultPort,
            };

            var connection = connectionFactory.CreateConnection();
            
            //HelloWorldExample.Exec(connection);

            #region Other examples
            //ProtoBufExample.Exec(connection);

            //RpcExample.Exec(connection);

            var bindingExample = new BindingExample(connection);
            bindingExample.Publish();
            bindingExample.Consume();
                        
            //var echoHostStarter = new EchoServiceHostStarter();
            //echoHostStarter.Start();

            //var duplexHostStarter = new MeetAndGreetServiceHostStarter();
            //duplexHostStarter.Start();
            #endregion

            connection.Close();
        }
    }
}
