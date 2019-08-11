using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GraphCore.Source.Events.Args;

namespace GraphCore.Source.Graph
{
    public class Graph
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
        private readonly ConcurrentBag<Position> _p;
        /// <summary>
        /// Represents global vertices count in graph
        /// </summary>
        public int VerticesCount => GetVerticesCount();
        /// <summary>
        /// Max possible vertices for non multi-vertices graph
        /// </summary>
        private readonly int _maxVerticesCount;
        /// <summary>
        /// There should be only one instance of random because of random.
        /// </summary>
        private readonly Random _random;



        // These events are not fully implemented, and are meant for external apps for usage.
        // Mainly Front-End UI can take advantage of these. 
        public EventHandler OnInitBegin;
        public EventHandler OnInitEnd;
        public EventHandler<EdgesInsertEventArgs> OnEdgesInsertingBegin;
        public EventHandler<EdgesInsertEventArgs> OnEdgesInsertingEnd;


        /// <summary>
        /// Init Graph instance
        /// </summary>
        /// <param name="n"> Node count (Positions) </param>
        /// <param name="initParallelism"> Max degree of init parallelism </param>
        public Graph(int n, int initParallelism = 4, int randomSeed = 42)
        {
            OnInitBegin?.Invoke(this, EventArgs.Empty);
            this._n = n;
            _matrix = new double[n, n];
            _p = new ConcurrentBag<Position>();
            _random = new Random(randomSeed);
            _maxVerticesCount = _n * (_n - 1) / 2;
            Parallel.For(0, _n, new ParallelOptions { MaxDegreeOfParallelism = initParallelism }, 
                (i, state) => _p.Add(new Position(_random.NextDouble(), _random.NextDouble())));
            OnInitEnd?.Invoke(this, EventArgs.Empty);
        }

        private int GetVerticesCount()
        {
            var localCount = 0; 
            // For each row, new thread. (Too much?)
            Parallel.For(0, _matrix.GetLength(0), new ParallelOptions(){MaxDegreeOfParallelism = _matrix.GetLength(0) }, (i,state) =>
            {
                for (var j = 0; j < i; j++)
                {
                    if (_matrix[i, j] > 0.0) Interlocked.Increment(ref localCount);
                }
            });
            return localCount;
        }

        /// <summary>
        /// Inserts random edge into graph.
        /// </summary>
        /// <param name="localThreadSafeRandom">To use local instance of random. (Thread-safe) </param>
        private void InsertRandomEdge(bool localThreadSafeRandom = false)
        {
            var threadSafeRandom = localThreadSafeRandom ? new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode())) : null;
            var random = localThreadSafeRandom ? threadSafeRandom.Value : _random;
            threadSafeRandom?.Dispose();
            while (true)
            {
                // Do not add another vertices, it max limit is reached. 
                if (GetVerticesCount() >= _maxVerticesCount) break;
                var fromVertex = random.Next(_n);
                var toVertex = random.Next(_n);
                if (fromVertex.Equals(toVertex))
                    continue;
                if (_matrix[fromVertex, toVertex] > 0.0)
                    continue;
                var dist = _p.ElementAt(fromVertex).QDistance(_p.ElementAt(toVertex));
                var prob = (2.0 - dist) * (2.0 - dist) / 4.0;
                if (!(random.NextDouble() < prob)) continue;
                _matrix[fromVertex, toVertex] = _matrix[toVertex, fromVertex] = Math.Sqrt(dist);
                break;
            }
        }
        
        /// <summary>
        /// Inserts n-random edges into graph. (Parallel)
        /// </summary>
        /// <param name="n">Vertices count (Cannot be bigger than [n*(n-1)/2], and will be lowered to this value otherwise. </param>
        public void InsertRandomEdges(int n)
        {
            OnEdgesInsertingBegin.Invoke(this, new EdgesInsertEventArgs(n));
            if (n > _maxVerticesCount) n = _maxVerticesCount;
            Parallel.For(0, n, (i, state) => InsertRandomEdge());
            OnEdgesInsertingEnd.Invoke(this, new EdgesInsertEventArgs(n));
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
    }
}
