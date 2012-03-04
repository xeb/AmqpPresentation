using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AmqpPresentation
{
    // ReSharper disable RedundantArgumentName
    public class BindingExample
    {
        private readonly IConnection _connection;

        public BindingExample(IConnection connection)
        {
            _connection = connection;    
        }
        private const string _exchangeName = "BindingExample";

        private static readonly string[] RoutingKeys = new[]
        {
            "wow.character.client.1234",
            "wow.character.client.5678",
            
            "wow.character.server.1234",
            "wow.character.server.5678",

            "wow.account.server.1234",
            "wow.account.server.5678",

            "wow.account.client.1234",
            "wow.account.client.5678",
        };

        private static readonly string[] Bindings = new[]
        {
            "wow.#", // all wow messages

            "wow.*", // WILL NOT MATCH

            "wow.*.*.*", // all wow messages

            //"wow.character.*.*", // all wow character messages
            
            //"*.character.*.*", // all messages about a character

            //"*.*.*.1234", // messages for connection 1234

            //"*.*.client.*", // all client messages

            //"*.*.client.5678", // all client messages for client 5678

            //"wow.character.server.5678",
        };

        private static void ExchangeDeclare(IModel model)
        {
            model.ExchangeDeclare(_exchangeName, type: "topic", durable: false); // No need for durability
        }

        private readonly System.Timers.Timer _publishTimer = new System.Timers.Timer();
        
        public void Publish()
        {
            _publishTimer.Enabled = true;
            _publishTimer.Interval = 5000;
            _publishTimer.Elapsed += OnPublish;

            Console.WriteLine("Setting up Timer to publish every {0}ms", _publishTimer.Interval);
        }

        private void OnPublish(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_connection) // We are just locking so the output looks orderly
            {
                try
                {
                    var model = _connection.CreateModel();

                    ExchangeDeclare(model);

                    foreach (var routingKey in RoutingKeys)
                    {
                        Console.WriteLine("Sending message to {0}", routingKey);

                        var msg = Encoding.UTF8.GetBytes(routingKey.ToUpper());
                        model.BasicPublish(_exchangeName, routingKey, null, msg);
                    }

                    Console.WriteLine("On Timer ({0})\r\n\r\n", Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception ex)
                {
                    WriteException(ex);
                }
            }
        }

        public void Consume()
        {
            Console.WriteLine("Starting up consumers...");

            foreach (var binding in Bindings)
            {
                string binding1 = binding;
                Task.Factory.StartNew(() =>
                {
                    try
                    {
                        Console.WriteLine("Waiting for messages matching {0}", binding1);
                        // Each thread should have its own model (or channel) -- but we still only have one socket open
                        var model = _connection.CreateModel();

                        // Declare the same exchange as the publisher
                        ExchangeDeclare(model);

                        // Create an exclusive queue
                        var exclusiveQueue = model.QueueDeclare();

                        // It's random so we need to get its' name
                        var queueName = exclusiveQueue.QueueName;

                        // Bind this queue to the exchange
                        model.QueueBind(queueName, _exchangeName, binding1);

                        // Define a basic queued consumer
                        var consumer = new QueueingBasicConsumer(model);

                        // Initiate consumption on the channel
                        model.BasicConsume(queueName, false, consumer);

                        // Sit & wait for a message
                        while (true)
                        {
                            // Dequeue something!  (will wait forever)
                            var item = consumer.Queue.Dequeue() as BasicDeliverEventArgs;
                            lock (_connection)
                                if (item != null)
                                    Console.WriteLine("Received {0} \t waiting for {1}", Encoding.UTF8.GetString(item.Body), binding1);
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteException(ex);
                    }
                });
            }
        }

        private void WriteException(Exception ex)
        {
            lock (_connection)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }
        }
    }
    // ReSharper restore RedundantArgumentName
}
