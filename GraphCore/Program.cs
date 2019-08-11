using System;
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
                var g = new Graph(n: 5, initParallelism: 4, randomSeed: 42);
                Console.WriteLine("Graph init: (zeros)");
                Console.WriteLine(g);
                g.OnEdgesInsertingBegin += OnEdgesInsertingBegin;
                g.OnEdgesInsertingEnd += OnEdgesInsertingEnd;
                g.InsertRandomEdges(200000);
                Console.WriteLine("Graph with edges:");
                Console.WriteLine(g);
            });
        }

        private static void OnEdgesInsertingEnd(object sender, EdgesInsertEventArgs e)
        {
            Console.WriteLine($"Graph edges inserted : [{e.EdgesCount}]");
        }

        private static void OnEdgesInsertingBegin(object sender, EdgesInsertEventArgs e)
        {
            Console.WriteLine($"Graph edges insertion begin: [{e.EdgesCount}]");
        }
    }
}
