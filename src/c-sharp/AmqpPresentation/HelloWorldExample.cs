using System;
using System.Text;
using RabbitMQ.Client;

namespace AmqpPresentation
{
    public static class HelloWorldExample
    {
        public static void Exec(IConnection connection)
        {
            var model = connection.CreateModel();
            const string queueName = "DestinationQueue";
            model.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine("Publishing!");
            for (int i = 0; i < 10; i++)
                model.BasicPublish("", // the exchange must never be null
                    queueName, null, Encoding.UTF8.GetBytes("Hello World!"));

            GetResult(queueName, model);
        }

        private static void GetResult(string queueName, IModel model)
        {
            var result = model.BasicGet(queueName, false);
            if (result == null)
            {
                Console.WriteLine("Nothing left!");
                return;
            }

            var msg = Encoding.UTF8.GetString(result.Body);
            Console.WriteLine("Received {0}", msg);

            Console.WriteLine("ACK'ing reciept of msg {0}", result.DeliveryTag);
            model.BasicAck(result.DeliveryTag, false);

            // Recursively get the other results
            GetResult(queueName, model);
        }
    }
}
