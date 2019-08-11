using System;
using System.Collections.Generic;
using System.Text;

namespace GraphCore.Source.Graph
{
    public class Position
    {
        private readonly double _x;
        private readonly double _y;

        public Position(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        public double QDistance(Position that)
        {
            return (this._x - that._x) * (this._x - that._x) + (this._y - that._y) * (this._y - that._y);
        }
    }
}
