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
                // Highest sensible n as nodes: 10000
                var g = new Graph(n: 8500, initParallelism: 1,  verticesParallelism:2, randomSeed: 42);
                //Console.WriteLine("Graph init: (zeros) \n" + g);
                Console.WriteLine(g.WriteMetaInfo());
                g.OnEdgesInsertingBegin += (sender, e) => Console.WriteLine($"Graph edges insert bgn: Count [{e?.VerticesCount}] : " +
                                                                            $"BeginTime [{e?.InvokeDateTime}]");
                g.OnEdgesInsertingEnd += (sender, e) => Console.WriteLine($"Graph edges insert end: Count [{e?.VerticesCount}] : " +
                                                                                  $"Inserted [{e?.InsertedVerticesCount}] : " +
                                                                                  $"BeginTime [{e?.InvokeDateTime}] : " +
                                                                                  $"Timespan [{e?.FromInvokeTimespan}]");
                // Comment for scarcer debug
                g.OnVerticessAddition += (sender, e) =>  Console.WriteLine($"Progress by: [{e?.CurrentChange}] => [{e?.TotalVertices}]");
                // Highest sensible n to insert: 100000
                g.InsertRandomEdges(10000);
                //Console.WriteLine("Graph with edges: \n" + g);
                Console.WriteLine(g.WriteMetaInfo());
            });
        }
    }
}
