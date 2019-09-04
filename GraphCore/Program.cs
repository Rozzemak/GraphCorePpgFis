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
            const int edgesToAdd = 20000;
            const bool threadSafeAdd = true;
            const int verticesParallelism = 2;
            const bool verbose = false;
            var gClassStack = new Graph<ConcurrentStack<Position>, Position>(n: nodeCount,
                (value, collection, random) => 
                    collection.Push(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gClassStack.SubscribeAndCompute(edgesToAdd, verbose, threadSafeAdd);
            
            var gStructStack = new Graph<ConcurrentStack<PositionStruct>, PositionStruct>(n: 10000,
                (value, collection, random) =>
                    collection.Push(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gStructStack.SubscribeAndCompute(edgesToAdd, verbose, threadSafeAdd);

            // ----------------------------------------------------------------------------------------

            var gClassQueue = new Graph<ConcurrentQueue<Position>, Position>(n: nodeCount,
                (value, collection, random) =>
                    collection.Enqueue(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gClassQueue.SubscribeAndCompute(edgesToAdd, verbose, threadSafeAdd);

            var gStructQueue = new Graph<ConcurrentQueue<PositionStruct>, PositionStruct>(n: nodeCount,
                (value, collection, random) =>
                    collection.Enqueue(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gStructQueue.SubscribeAndCompute(edgesToAdd, verbose, threadSafeAdd);

            // ---------------------------------------------------------------------------------------

            var gClassBag = new Graph<ConcurrentBag<Position>, Position>(n: nodeCount,
                (value, collection, random) =>
                    collection.Add(new Position(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gClassBag.SubscribeAndCompute(edgesToAdd, verbose, threadSafeAdd);

            var gStructBag = new Graph<ConcurrentBag<PositionStruct>, PositionStruct>(n: nodeCount,
                (value, collection, random) =>
                    collection.Add(new PositionStruct(random.NextDouble(), random.NextDouble())),
                initParallelism: 1, verticesParallelism, randomSeed: 42);
            gStructBag.SubscribeAndCompute(edgesToAdd, verbose, threadSafeAdd);

        }
    }
}
