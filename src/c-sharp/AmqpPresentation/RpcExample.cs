﻿using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.v0_9_1;

namespace AmqpPresentation
{
    // ReSharper disable RedundantArgumentName
    public static class RpcExample
    {
        private const string _incomingRequests = "RequestQueue";

        private static IConnection _connection;

        public static void Exec(IConnection connection)
        {
            _connection = connection;

            StartListening(_connection.CreateModel());
            StartSending();
            
            Thread.Sleep(40000);
        }

        public static void StartListening(IModel model)
        {
            DeclareQueue(model);

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var consumer = new QueueingBasicConsumer(model);

                    model.BasicConsume(queue: _incomingRequests, noAck: false, consumer: consumer);

                    Console.WriteLine("Waiting for requests...");
                    Receive(model, consumer);
                }
                catch (Exception ex)
                {
                    WriteException(ex);   
                }
            });
        }

        private static void Receive(IModel model, QueueingBasicConsumer consumer)
        {
            var request = consumer.Queue.Dequeue() as BasicDeliverEventArgs;
            if (request == null)
            {
                Console.WriteLine("Request is NULL!");
                return;
            }
            else
            {
                var requestObj = request.Body.Deserialize<GetDateRequest>();
                Console.WriteLine("[Server] Received Request: {0}", requestObj.ToJson());

                // Ack the request
                model.BasicAck(request.DeliveryTag, false);

                // Create a reply
                var replyQueue = request.BasicProperties.ReplyTo;
                var correlationId = request.BasicProperties.CorrelationId;

                var replyObj = new GetDateResponse {Timestamp = DateTime.Now};
                var replyProperties = new BasicProperties
                {
                    CorrelationId = correlationId,
                };

                // Send the reply
                Console.WriteLine("[Server] Sending reply...");
                model.BasicPublish("", replyQueue, replyProperties, replyObj.Serialize());

                // Now receive the next message
                Receive(model, consumer);
            }
        }

        private static readonly System.Timers.Timer EchoTimer = new System.Timers.Timer(4500);

        public static void StartSending()
        {
            EchoTimer.Enabled = true;
            EchoTimer.Elapsed += Send;
        }

        public static void Send(object sender, System.Timers.ElapsedEventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    if (_connection == null)
                        return;

                    // create a new channel in this thread
                    var sendChannel = _connection.CreateModel();

                    // new correlation ID for this request
                    var myCorrelationId = Guid.NewGuid().ToString();

                    // declare a queue for me to read the response
                    var myQueue = sendChannel.QueueDeclare();

                    // set the correlationId and replyTo queue in the basic properties (this could be done in the Body/Application Layer, but the features are built into AMQP)
                    var requestProperties = new BasicProperties
                    {
                        ReplyTo = myQueue.QueueName,
                        CorrelationId = myCorrelationId,
                    };

                    // Publish a send message
                    sendChannel.BasicPublish("", _incomingRequests, requestProperties, new GetDateRequest { Name = myCorrelationId }.Serialize());
                    Console.WriteLine("[Client] Sent request.  Correlation ID {0}", myCorrelationId);

                    // Now we need to wait for a reply
                    var queuedConsumer = new QueueingBasicConsumer(sendChannel);
                    sendChannel.BasicConsume(myQueue.QueueName, false, queuedConsumer);

                    // Now we wait for a response!
                    var response = queuedConsumer.Queue.Dequeue() as BasicDeliverEventArgs;
                    if (response == null)
                        throw new InvalidOperationException("No response!");

                    var correlationId = response.BasicProperties.CorrelationId;
                    Console.WriteLine("[Client] Received reply for {0}", correlationId);

                    // Deserialize the repsonse
                    var responseObj = response.Body.Deserialize<GetDateResponse>();
                    Console.WriteLine("[Client] Received timestamp {0} it is now {1}\r\n----------------------------------\r\n\r\n", responseObj.Timestamp, DateTime.Now);
                }
                catch (Exception ex)
                {
                    WriteException(ex);
                }
            });
        }

        private static void DeclareQueue(IModel model)
        {
            model.QueueDeclare(queue: _incomingRequests, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        private static void WriteException(Exception ex)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
            }
        }
    }
    // ReSharper restore RedundantArgumentName

    [DataContract]
    public class GetDateRequest
    {
        [DataMember(Order = 1)]
        public string Name { get; set; }
    }
    
    [DataContract]
    public class GetDateResponse
    {
        [DataMember(Order = 1)]
        public DateTime Timestamp { get; set; }
    }
}
