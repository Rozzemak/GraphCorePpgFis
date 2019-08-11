using System;
using System.Threading.Tasks;
using GraphCore.Source.Graph;

namespace GraphCore
{
    public static class Program
    {
        public static Task Main(string[] args)
        {
            return Task.Run(() =>
            {
                var g = new Graph(20);
                g.InsertRandomEdges(45);
                Console.WriteLine(g);
            });
        }
    }
}
