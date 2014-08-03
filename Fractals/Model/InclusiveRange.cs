﻿using System;

namespace Fractals.Model
{
    public sealed class InclusiveRange
    {
        public readonly double Minimum;
        public readonly double Maximum;

        public double Magnitude
        {
            get { return Math.Abs(Maximum - Minimum); }
        }

        public InclusiveRange(double minimum, double maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public bool IsInside(double value)
        {
            return
                value >= Minimum &&
                value <= Maximum;
        }
    }
}
