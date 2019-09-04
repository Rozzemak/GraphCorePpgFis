using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GraphCore.Source.Events.Args;
using GraphCore.Source.Graph;

namespace GraphCore
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // Highest sensible n as nodes: 10000
            var gClassStack = new Graph<ConcurrentStack<Position>, Position>(n: 10000,
                (value, collection, random) => 
                    collection.Push(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism: 2, randomSeed: 42);
            //Console.WriteLine("Graph init: (zeros) \n" + g);

            gClassStack.SubscribeAndCompute(10000);
            var gStructStack = new Graph<ConcurrentStack<PositionStruct>, PositionStruct>(n: 10000,
                (value, collection, random) =>
                    collection.Push(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism: 2, randomSeed: 42);
            gStructStack.SubscribeAndCompute(10000);
            // ---------------------------------------------------------------------------------------

            var gClassBag = new Graph<ConcurrentBag<Position>, Position>(n: 10000,
                (value, collection, random) =>
                    collection.Add(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism: 2, randomSeed: 42);
            //Console.WriteLine("Graph init: (zeros) \n" + g);
            gClassBag.SubscribeAndCompute(10000);

            var gStructBag = new Graph<ConcurrentBag<PositionStruct>, PositionStruct>(n: 10000,
                (value, collection, random) =>
                    collection.Add(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism: 2, randomSeed: 42);
            gStructBag.SubscribeAndCompute(10000);
        }
    }
}
