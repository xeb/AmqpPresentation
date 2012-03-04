using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using ProtoBuf;
using RabbitMQ.Client;

namespace AmqpPresentation
{
    public static class ProtoBufExample
    {
        public static void Exec(IConnection connection)
        {
            var model = connection.CreateModel();
            const string queueName = "DestinationQueue";
            model.QueueDeclare(queueName, true, false, false, null);

            Console.WriteLine("Publishing!");
            //for (int i = 0; i < 10; i++)
            model.BasicPublish("", queueName, null, new SomeMessage { Timestamp = DateTime.Now }.Serialize());

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

            var msg = result.Body.Deserialize<SomeMessage>().ToJson();
            Console.WriteLine("Received {0}", msg);

            Console.WriteLine("ACK'ing reciept of msg {0}", result.DeliveryTag);
            model.BasicAck(result.DeliveryTag, false);

            // Recursively get the other results
            GetResult(queueName, model);
        }
    }

    [DataContract]
    public class SomeMessage
    {
        [DataMember]
        public DateTime Timestamp { get; set; }
    }
}
