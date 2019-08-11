using System;
using System.Collections.Generic;
using System.Text;

namespace GraphCore.Source.Events.Args
{
    public class EdgesInsertEventArgs : EventArgs
    {
        public readonly int EdgesCount;

        public EdgesInsertEventArgs(int edgesCount)
        {
            this.EdgesCount = edgesCount;
        }
    }
}
