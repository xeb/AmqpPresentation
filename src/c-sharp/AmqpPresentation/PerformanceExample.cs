using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing.v0_9_1;

namespace AmqpPresentation
{
    public static class PerformanceExample
    {
        private const string _overrideQueueName = "Intersystem";
        private const bool _publish = true;
        private const bool _receive = true;
        private const int _numMsgs = 1000;

        // ReSharper disable ConditionIsAlwaysTrueOrFalse

        public static void Exec(bool durable, bool ack, IModel model, IConnection connection)
        {
            var queueName = "CSharpPublish" + (durable ? "Durable" : "NotDurable");
            if (!string.IsNullOrWhiteSpace(_overrideQueueName))
                queueName = _overrideQueueName;

            Console.WriteLine("Durability {0} enabled", durable ? "IS" : "is NOT");
            Console.WriteLine("ACK {0} enabled", ack ? "IS" : "is NOT");

            Console.WriteLine("Creating queue {0}...", queueName);
            model.QueueDeclare(queueName, durable, false, false, null);

            var msg = Encoding.UTF8.GetBytes("Hello from my PC !");
            var properties = new BasicProperties { DeliveryMode = (byte)(durable ? 2 : 1) };

            if (_publish)
            {
                for (int i = 0; i < _numMsgs; i++)
                    model.BasicPublish(string.Empty, queueName, properties, msg);


                Console.WriteLine("Published!");
            }

            uint msgs = 0;
            if (_receive)
            {
                Console.WriteLine("Reading...");
                var fromMac = false;
                while (connection.IsOpen)
                {
                    var result = model.BasicGet(queueName, ack == false);
                    if (result == null)
                        break;

                    if (msgs % 3144 == 0)
                        Console.WriteLine(msgs);

                    if (ack)
                        model.BasicAck(result.DeliveryTag, false);
                    var msgText = Encoding.UTF8.GetString(result.Body);
                    if (msgText.Contains("Mac"))
                        fromMac = true;
                    msgs++;
                }

                if (fromMac)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("Received messages from the mac");
                    Console.ResetColor();
                }
            }
        }

        // ReSharper restore ConditionIsAlwaysTrueOrFalse
    }

}
