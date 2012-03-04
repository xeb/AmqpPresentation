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

            var msg = SomeMessage.Deserialize(result.Body).ToJson();
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

        public byte[] Serialize()
        {
            using(var ms = new MemoryStream())
            {
                Serializer.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        public static SomeMessage Deserialize(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return Serializer.Deserialize<SomeMessage>(ms);
            }
        }
    }

    public static class JsonExtensions
    {
        public static string ToJson<T>(this T obj)
        {
            using (var ms = new MemoryStream())
            {
                var dc = new DataContractJsonSerializer(typeof(T));
                dc.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static T FromJson<T>(this string json)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                var dc = new DataContractJsonSerializer(typeof(T));
                return (T)dc.ReadObject(ms);
            }
        }
    }
}
