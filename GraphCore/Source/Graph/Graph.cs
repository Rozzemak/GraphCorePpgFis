using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// There should be only one instance of random because of random.
        /// </summary>
        private readonly Random _random;


        // These events are not fully implemented, and are meant for external apps for usage.
        // Mainly Front-End UI can take advantage of these. 
        public readonly EventHandler OnInitBegin;
        public readonly EventHandler OnInitEnd;
        public readonly EventHandler OnEdgesInsertingBegin;
        public readonly EventHandler OnEdgesInsertingEnd;


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
            Parallel.For(0, _n, new ParallelOptions { MaxDegreeOfParallelism = initParallelism }, 
                (i, state) => _p.Add(new Position(_random.NextDouble(), _random.NextDouble())));
            OnInitEnd?.Invoke(this, EventArgs.Empty);
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
        /// <param name="n">Edges count</param>
        public void InsertRandomEdges(int n)
        {
            OnEdgesInsertingBegin.Invoke(this, EventArgs.Empty);
            Parallel.For(0, n, (i, state) => InsertRandomEdge());
            OnEdgesInsertingEnd.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// String override of Graph class.
        /// </summary>
        /// <returns>String representation of graph class. (Humanized)</returns>
        public override string ToString()
        {
            var s = new StringBuilder();
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
