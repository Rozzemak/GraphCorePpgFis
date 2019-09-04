using System;
using System.Collections.Generic;
using System.Text;

namespace GraphCore.Source.Graph
{
    public struct PositionStruct : IPosition<PositionStruct>
    {
        private readonly double _x;
        private readonly double _y;

        public PositionStruct(double x, double y)
        {
            this._x = x;
            this._y = y;
        }

        public double QDistance(PositionStruct that)
        {
            return (this._x - that._x) * (this._x - that._x) + (this._y - that._y) * (this._y - that._y);
        }
    }

    public class Position : IPosition<Position>
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

    public interface IPosition<in TPos>
    {
        double QDistance(TPos that);
    }
}
