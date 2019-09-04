using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphCore.Source.Events.Args;

namespace GraphCore.Source.Graph
{
    /// <summary>
    /// Graph class
    /// </summary>
    /// <typeparam name="TThreadSaveCollection">Thread-safe collection of positions</typeparam>
    /// <typeparam name="TPos">Position type</typeparam>
    public class Graph<TThreadSaveCollection, TPos> 
        where TThreadSaveCollection : IProducerConsumerCollection<TPos>, new()
        where TPos : IPosition<TPos>
    {
        /// <summary>
        /// Node count
        /// </summary>
        private readonly int _n;
        /// <summary>
        /// Graph matrix (Nodes and vertices)
        /// </summary>
        private readonly double[,] _matrix;
        /// <summary>
        /// Thread-safe collection of vertices
        /// </summary>
        private readonly TThreadSaveCollection _p;
        /// <summary>
        /// Represents global vertices count in graph
        /// </summary>
        public int VerticesCount => GetVerticesCountSafe();
        /// <summary>
        /// Max possible vertices for non multi-vertices graph
        /// </summary>
        private readonly ulong _maxVerticesCount;
        /// <summary>
        /// Parallel pool for GetVertices method
        /// </summary>
        private readonly int _verticesParallelism;
        /// <summary>
        /// Parallel pool for Init method
        /// </summary>
        private readonly int _initParallelism;
        /// <summary>
        /// There should be only one instance of random because of random.
        /// </summary>
        private readonly Random _random;


        // These events are not fully implemented, and are meant for external apps for usage.
        // Mainly Front-End UI can take advantage of these. 
        // (Not possible to sub them before calling constructor, but could be use-full for  re-init.)
        public EventHandler OnInitBegin;
        public EventHandler OnInitEnd;

        /// <summary>
        /// Invoked on InsertRandomEdges invocation. (Before method logic)
        /// </summary>
        public EventHandler<EdgesInsertEventArgs> OnEdgesInsertingBegin;
        /// <summary>
        /// Invoked after InsertRandomEdges invocation. (After method logic)
        /// </summary>
        public EventHandler<EdgesInsertEventArgs> OnEdgesInsertingEnd;
        /// <summary>
        /// Invoked after new set of vertices has been added. (Per worker group)
        /// </summary>
        public EventHandler<VerticesProgressEventArgs> OnVerticessAddition;

        /// <summary>
        /// Init Graph instance
        /// </summary>
        /// <param name="n"> Node count (Positions) </param>
        /// <param name="initParallelism"> Max degree of init parallelism </param>
        /// <param name="randomSeed"> Self explanatory</param>
        public Graph([NotNull] uint n, Action<int, TThreadSaveCollection, Random> addToCollection, [Range(1, 255)] int initParallelism = 4, int randomSeed = 42)
        {
            OnInitBegin?.Invoke(this, EventArgs.Empty);
            this._n = (int)n;
            _matrix = new double[_n, _n];
            _p = new TThreadSaveCollection();
            _random = new Random(randomSeed);
            _maxVerticesCount = ((ulong)_n * ((ulong)_n - 1) / 2);
            this._verticesParallelism = initParallelism*2;
            this._initParallelism = initParallelism;
            Init(ref initParallelism, addToCollection);
            OnInitEnd?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Init Graph instance
        /// </summary>
        /// <param name="n"> Node count (Positions) </param>
        /// <param name="addToCollection"></param>
        /// <param name="initParallelism"> Max degree of init parallelism </param>
        /// <param name="verticesParallelism">Vertices parallelism pool</param>
        /// <param name="randomSeed"> Self explanatory</param>
        public Graph([NotNull] uint n, Action<int, TThreadSaveCollection, Random> addToCollection, [NotNull] [Range(1, 255)] int initParallelism,
            [NotNull] [Range(1, 255)] int verticesParallelism, int randomSeed = 42)
        {
            OnInitBegin?.Invoke(this, EventArgs.Empty);
            this._n = (int)n;
            _matrix = new double[_n, _n];
            _p = new TThreadSaveCollection();
            _random = new Random(randomSeed);
            _maxVerticesCount = ((ulong)_n * ((ulong)_n - 1) / 2);
            this._verticesParallelism = verticesParallelism;
            this._initParallelism = initParallelism;
            Init(ref initParallelism, addToCollection);
            OnInitEnd?.Invoke(this, EventArgs.Empty);

        }

        /// <summary>
        /// Init of graph class for code reduction. (Lots of readonly variables, not too useful.)
        /// Could be async though. Or at least invoked as task.
        /// </summary>
        /// <param name="initParallelism"></param>
        /// <param name="addToCollection"></param>
        private void Init(ref int initParallelism, Action<int, TThreadSaveCollection, Random> addToCollection)
        {
            //_p.Push(new Position(_random.NextDouble(), _random.NextDouble()))
#if DEBUG
            Console.WriteLine("Project configuration mode is set to debug. " +
                              "Operations will take LOT longer to finish. (Quadratic/Log time increase?)");
#endif
            Parallel.For(0, _n, new ParallelOptions { MaxDegreeOfParallelism = initParallelism },
                i => addToCollection(i, _p ,_random));
        }

        /// <summary>
        /// Gets graph vertices count.
        /// </summary>
        /// <returns></returns>
        private int GetVerticesCountSafe()
        {
            var localCount = 0;
            // For each row, new thread. (Too much?)
            lock (_matrix)
                Parallel.For(0, _matrix.GetLength(0), new ParallelOptions() { MaxDegreeOfParallelism = _verticesParallelism }, i =>
                  {
                      for (var j = 0; j < i + 1; j++)
                      {
                          if (_matrix[i, j] > 0.0) Interlocked.Increment(ref localCount);
                      }
                  });
            return localCount;
        }
        /// <summary>
        /// Inserts random edge into graph.
        /// Has options for locking, though it is random vertices adding, so if few are missed, nothing happens.
        /// </summary>
        /// <param name="totalVertices"> Inserted vertices counter (For ParFor)</param>
        /// <param name="localThreadSafeRandom">To use local instance of random. (Thread-safe) </param>
        /// <param name="lockMatrix">Lock or not to lock graph matrix. False will produce few errors, but is generally much faster</param>
        private int InsertRandomEdge(ref int totalVertices, bool localThreadSafeRandom = false, bool lockMatrix = true)
        {
            var insertedVertices = 0;
            var threadSafeRandom = localThreadSafeRandom ? new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode())) : null;
            var random = localThreadSafeRandom ? threadSafeRandom.Value : _random;
            threadSafeRandom?.Dispose();
            while (true)
            {
                // This is thread safe, but VERY SLOW
                // if (VerticesCount >= _maxVerticesCount) break;
                if (totalVertices >= (int) _maxVerticesCount) break;
                if (lockMatrix)
                {
                    lock (_matrix)
                    {
                        var fromVertex = random.Next(_n);
                        var toVertex = random.Next(_n);
                        if (fromVertex.Equals(toVertex))
                            continue;
                        if (CalculateVerticesDistance(ref insertedVertices, ref fromVertex, ref toVertex, ref random)) continue;
                    }
                }
                else
                {
                    var fromVertex = random.Next(_n);
                    var toVertex = random.Next(_n);
                    if (fromVertex.Equals(toVertex))
                        continue;
                    if (CalculateVerticesDistance(ref insertedVertices, ref fromVertex, ref toVertex, ref random)) continue;
                }
                break;
            }
            return insertedVertices;
        }

        /// <summary>
        /// Logic to calculate random distance between two graph nodes.
        /// </summary>
        /// <param name="insertedVertices"></param>
        /// <param name="fromVertex"></param>
        /// <param name="toVertex"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private bool CalculateVerticesDistance(ref int insertedVertices, ref int fromVertex, ref int toVertex, ref Random random)
        {
            if (_matrix[fromVertex, toVertex] > 0.0) return true;
            var dist = _p.ElementAt(fromVertex).QDistance(_p.ElementAt(toVertex));
            var prob = (2.0 - dist) * (2.0 - dist) / 4.0;
            if (!(random.NextDouble() < prob)) return true;
            var val = Math.Sqrt(dist);
            Interlocked.Exchange(ref _matrix[fromVertex, toVertex], val);
            Interlocked.Exchange(ref _matrix[toVertex, fromVertex], val);
            Interlocked.Increment(ref insertedVertices);
            return false;
        }

        /// <summary>
        /// Inserts n-random edges into graph. (Parallel)
        /// Shares par. pool with vertices getter  (Laziness on my part)
        /// </summary>
        /// <param name="n">Vertices count (Cannot be bigger than [n*(n-1)/2], and will be lowered to this value otherwise. </param>
        /// <param name="threadSafe">Add edges to graph safely</param>
        public void InsertRandomEdges(int n, bool threadSafe)
        {
            var args = new EdgesInsertEventArgs(n);
            // This is thread-safe, cannot be called in inner loops.
            var totalVertices = GetVerticesCountSafe();
            Task.Run(() => OnEdgesInsertingBegin.Invoke(this, args));
            if (n > (int)_maxVerticesCount) n = (int)_maxVerticesCount;
            Parallel.For(0, n, new ParallelOptions(){MaxDegreeOfParallelism = _verticesParallelism},
                () => 0, (pr, ad, s) => Interlocked.Add(ref s, InsertRandomEdge(ref totalVertices, false, threadSafe)), 
                poolResult =>
                {
                    Interlocked.Add(ref totalVertices, poolResult);
                    OnVerticessAddition?.Invoke(this, new VerticesProgressEventArgs(totalVertices,  poolResult));
                });
            OnEdgesInsertingEnd.Invoke(this, new EdgesInsertEventArgs(n, args.InvokeDateTime, totalVertices));
        }

        public void SubscribeAndCompute(int edges, bool threadSafe = true)
        {
            Console.WriteLine(WriteMetaInfo());
            OnEdgesInsertingBegin += (sender, e) => Console.WriteLine($"Graph edges insert bgn: Count [{e?.VerticesCount}] : " +
                                                                            $"BeginTime [{e?.InvokeDateTime}]");
            OnEdgesInsertingEnd += (sender, e) => Console.WriteLine($"Graph edges insert end: Count [{e?.VerticesCount}] : " +
                                                                          $"Inserted [{e?.InsertedVerticesCount}] : " +
                                                                          $"BeginTime [{e?.InvokeDateTime}] : " +
                                                                          $"Timespan [{e?.FromInvokeTimespan}]");
            // Comment for scarcer debug
            OnVerticessAddition += (sender, e) => Console.WriteLine($"Progress by: [{e?.CurrentChange}] => [{e?.TotalVertices}]");
            // Highest sensible n to insert: 100000
            InsertRandomEdges(edges, threadSafe);
            //Console.WriteLine("Graph with edges: \n" + g);
            Console.WriteLine(WriteMetaInfo());
        }

        /// <summary>
        /// String override of Graph class.
        /// </summary>
        /// <returns>String representation of graph class. (Humanized)</returns>
        public override string ToString()
        {
            var s = new StringBuilder();
            s.Append($"Nodes : [{_n}] ,Vertices : [{VerticesCount}] \n");
            for (var i = 0; i < _n; i++)
            {
                for (var j = 0; j < _n; j++)
                {
                    s.Append($"{_matrix[i, j]:0.0}  ");
                }
                s.Append("\n");
            }
            return s.ToString();
        }

        private string WriteMetaInfo()
        {
            return $"Graph<{(typeof(TThreadSaveCollection).GetGenericArguments()[0]).Name}>" +
                   $" => Parallelism : (Init[{_initParallelism}], Vertices[{_verticesParallelism}]) , " +
                   $"Nodes : [{_n}] , " +
                   $"Vertices : [{VerticesCount}]";
        }
    }
}
