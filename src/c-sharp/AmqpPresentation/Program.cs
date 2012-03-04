using System;
using System.Threading;
using RabbitMQ.Client;

namespace AmqpPresentation
{
    public class Program
    {
        private const string _broker = "localhost";

        public static void Main(string[] args)
        {
            Console.WriteLine("Connecting to {0}...", _broker);
            var connectionFactory = new ConnectionFactory
            {
                Endpoint = new AmqpTcpEndpoint(_broker),
                Protocol = Protocols.FromEnvironment(),
                Port = AmqpTcpEndpoint.UseDefaultPort,       
            };

            var connection = connectionFactory.CreateConnection();

            HelloWorldExample.Exec(connection);

            var bindingExample = new BindingExample(connection);
            bindingExample.Publish();
            bindingExample.Consume();
            // Just keep running for awhile...
            Thread.Sleep(500000);

            connection.Close();
        }
    }
}
