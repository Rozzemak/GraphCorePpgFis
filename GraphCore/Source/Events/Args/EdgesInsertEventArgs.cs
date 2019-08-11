using System;
using System.Collections.Generic;
using System.Text;

namespace GraphCore.Source.Events.Args
{
    public class EdgesInsertEventArgs : EventArgs
    {
        public readonly int VerticesCount;
        public readonly int InsertedVerticesCount;
        public readonly DateTime InvokeDateTime;
        public readonly TimeSpan FromInvokeTimespan;
        public EdgesInsertEventArgs(int verticesCount)
        {
            this.VerticesCount = verticesCount;
            this.InvokeDateTime = DateTime.Now;
        }

        public EdgesInsertEventArgs(int verticesCount, DateTime fromTimedInvoke, int insertedVertices)
        {
            this.VerticesCount = verticesCount;
            this.InvokeDateTime = DateTime.Now;
            this.FromInvokeTimespan = InvokeDateTime - fromTimedInvoke;
            this.InsertedVerticesCount = insertedVertices;
        }
    }
}

