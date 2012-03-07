using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using WcfBindingPerformance.Services;

namespace RabbitMQDuplexTest
{
    public class Program
    {
        private const int _numberThreads = 1;
        private const int _iterations = 1000;
        private const int _batches = 1;

        public static void Main(string[] args)
        {
            var sh = new ServiceHost(typeof (DuplexRabbitService), new Uri("http://localhost:333/Rabbit"));
            sh.Open();

            var results = RunTest();
            Console.WriteLine(results);
            Thread.Sleep(1000000);
            sh.Close();
        }

        private static string RunTest()
        {
            var svc = new DuplexRabbitClient(false);
            var sw = new Stopwatch();
            sw.Start();

            var results = new List<double>();
            var errors = new List<Exception>();

            for (int j = 0; j < _iterations; j++)
            {
                //Console.WriteLine(j);
                var isw = new Stopwatch();
                isw.Start();

                for (int l = 0; l < _batches; l++)
                {
                    var tasks = new List<Task>();

                    for (int i = 0; i < _numberThreads; i++)
                    {
                        tasks.Add(Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                svc.GetDate(new GetDateRequest { RequestTime = DateTime.Now });
                            }
                            catch (Exception ex)
                            {
                                lock (errors)
                                {
                                    errors.Add(ex);
                                }
                            }
                        }));
                    }

                    Task.WaitAll(tasks.ToArray());
                }

                isw.Stop();
                results.Add(isw.ElapsedMilliseconds);
            }

            sw.Stop();

            var content = svc.GetType().FullName + "\r\n" +
                string.Join("\r\n", results) +
                "\r\n\r\nTotal:\t\t" + sw.ElapsedMilliseconds +
                "\r\n\r\nAverage:\t" + results.Average() +
                "\r\n\r\nErrors:\t" + errors.Count() +
                "\r\n\r\n";

            return content;            
        }
    }
}
