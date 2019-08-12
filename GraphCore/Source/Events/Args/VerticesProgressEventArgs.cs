using System;
using System.Collections.Generic;
using System.Text;

namespace GraphCore.Source.Events.Args
{
    public class VerticesProgressEventArgs : EventArgs
    {
        public readonly int TotalVertices;
        public readonly int CurrentChange;

        public VerticesProgressEventArgs(int totalVertices, int currentChange)
        {
            this.TotalVertices = totalVertices;
            this.CurrentChange = currentChange;
        }
    }
}
