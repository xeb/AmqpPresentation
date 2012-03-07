using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
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
        private const int _frequency = 5000;
        private readonly IConnection _connection;
        private readonly ConcurrentBag<IModel> _models = new ConcurrentBag<IModel>();
        private const bool _writeToConsole = true;
        private const bool _ack = false;
        
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

            "*.*.*.1234", // messages for connection 1234

            //"*.*.client.*", // all client messages

            //"*.*.client.5678", // all client messages for client 5678

            //"wow.character.server.5678",
        };

        private static void ExchangeDeclare(IModel model)
        {
            model.ExchangeDeclare(_exchangeName, type: "topic", durable: false); // No need for durability
        }

        private IModel GetModel()
        {
            IModel model;
            if (_models.TryTake(out model))
                return model;

            return _connection.CreateModel();
        }

        private void ReturnModel(IModel model)
        {
            _models.Add(model);
        }

        private readonly System.Timers.Timer _publishTimer = new System.Timers.Timer();
        
        public void Publish()
        {
            _publishTimer.Enabled = true;
            _publishTimer.Interval = _frequency;
            _publishTimer.Elapsed += OnPublish;

            Console.WriteLine("Setting up Timer to publish every {0}ms", _publishTimer.Interval);
        }

        private void OnPublish(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_connection) // We are just locking so the output looks orderly
            {
                try
                {
                    var model = GetModel();
                    
                    if(_writeToConsole)
                        Console.WriteLine("\r\n\r\n\r\n\r\n------------------------------------------------");
                    
                    ExchangeDeclare(model);

                    foreach (var routingKey in RoutingKeys)
                    {
                        if (_writeToConsole)
                            Console.WriteLine("Sending message to {0}", routingKey);

                        var msg = Encoding.UTF8.GetBytes("{" + string.Format("Body: {0}", routingKey) + "}");
                        model.BasicPublish(_exchangeName, routingKey, null, msg);
                    }

                    if (_writeToConsole)
                        Console.WriteLine("\r\nOn Timer ({0})\r\n", Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception ex)
                {
                    WriteException(ex);
                }
            }
        }

        public void Consume()
        {
            if (_writeToConsole)
                Console.WriteLine("Starting up consumers...");
            var tasks = new List<Task>();
            foreach (var binding in Bindings)
            {
                string binding1 = binding;
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        if (_writeToConsole)
                            Console.WriteLine("Waiting for messages matching {0}", binding1);
                        // Each thread should have its own model (or channel) -- but we still only have one socket open
                        var model = GetModel();

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
                                {
                                    if (_writeToConsole)
                                        Console.WriteLine("Received {0} \t waiting for {1}", Encoding.UTF8.GetString(item.Body), binding1);

                                    // --------- NOTE ABOUT EXCLUSIVE QUEUES AND ACKING-------------------

                                    if(_ack)
                                        model.BasicAck(item.DeliveryTag, false);
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteException(ex);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());
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
