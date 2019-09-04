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
        public static void Main(string[] args)
        {
            // Highest sensible n as nodes: 10000
            const int nodeCount = 10000;
            const int edgesToAdd = 100000;
            const bool threadSafeAdd = false;
            const int verticesParallelism = 2;
            var gClassStack = new Graph<ConcurrentStack<Position>, Position>(n: nodeCount,
                (value, collection, random) => 
                    collection.Push(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gClassStack.SubscribeAndCompute(edgesToAdd, threadSafeAdd);

            var gStructStack = new Graph<ConcurrentStack<PositionStruct>, PositionStruct>(n: 10000,
                (value, collection, random) =>
                    collection.Push(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gStructStack.SubscribeAndCompute(edgesToAdd, threadSafeAdd);
            // ---------------------------------------------------------------------------------------

            var gClassBag = new Graph<ConcurrentBag<Position>, Position>(n: nodeCount,
                (value, collection, random) =>
                    collection.Add(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gClassBag.SubscribeAndCompute(edgesToAdd, threadSafeAdd);

            var gStructBag = new Graph<ConcurrentBag<PositionStruct>, PositionStruct>(n: nodeCount,
                (value, collection, random) =>
                    collection.Add(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gStructBag.SubscribeAndCompute(edgesToAdd, threadSafeAdd);
        }
    }
}
