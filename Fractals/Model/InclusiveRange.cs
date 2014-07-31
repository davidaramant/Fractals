using System;

namespace Fractals.Model
{
    public sealed class InclusiveRange
    {
        public readonly double Min;
        public readonly double Max;

        public double Magnitude
        {
            get { return Math.Abs(Max - Min); }
        }

        public InclusiveRange(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public bool IsInside(double value)
        {
            return
                value >= Min &&
                value <= Max;
        }
    }
}
