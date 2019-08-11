using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GraphCore.Source.Events.Args;
using GraphCore.Source.Graph;

namespace GraphCore
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            return Task.Run(() =>
            {
                var g = new Graph(n: 7, initParallelism: 10,  10, randomSeed: 42);
                Console.WriteLine("Graph init: (zeros) \n" + g);
                Console.WriteLine(g.WriteMetaInfo());
                g.OnEdgesInsertingBegin += OnEdgesInsertingBegin;
                g.OnEdgesInsertingEnd += OnEdgesInsertingEnd;
                g.InsertRandomEdges(20000);
                Console.WriteLine("Graph with edges: \n" + g);
                Console.WriteLine(g.WriteMetaInfo());
            });
        }

        private static void OnEdgesInsertingEnd(object sender, EdgesInsertEventArgs e)
        {
            Console.WriteLine($"Graph edges insert end: Count [{e.VerticesCount}] : " +
                              $"Inserted [{e.InsertedVerticesCount}] : " +
                              $"BeginTime [{e.InvokeDateTime}] : " +
                              $"Timespan [{e.FromInvokeTimespan}]");
        }

        private static void OnEdgesInsertingBegin(object sender, EdgesInsertEventArgs e)
        {
            Console.WriteLine($"Graph edges insert bgn: Count [{e.VerticesCount}] : BeginTime [{e.InvokeDateTime}]");
        }
    }
}
