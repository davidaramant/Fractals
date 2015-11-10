using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fractals.Utility
{
    public sealed class ColorGradient : IEnumerable<ColorGradient.ColorRange>
    {
        private readonly List<ColorRange> _colorRanges = new List<ColorRange>();

        public ColorGradient(IEnumerable<Tuple<HsvColor, double>> colorPoints)
        {
            var temp = colorPoints.ToArray();

            for (int i = 0; i < temp.Length - 1; i++)
            {
                var current = temp[i];
                var next = temp[i + 1];

                _colorRanges.Add(new ColorRange(
                    startColor: current.Item1,
                    start: current.Item2,
                    endColor: next.Item1,
                    end: next.Item2));
            }
        }

        public HsvColor GetColor(double ratio)
        {
            var range = _colorRanges.First(r => r.IsInsideRange(ratio));

            return range.Interpolate(ratio);
        }

        public IEnumerator<ColorRange> GetEnumerator()
        {
            return _colorRanges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public sealed class ColorRange
        {
            public HsvColor StartColor { get; }
            public HsvColor EndColor { get; }
            public double Start { get; }
            public double End { get; }

            public ColorRange(HsvColor startColor, HsvColor endColor, double start, double end)
            {
                StartColor = startColor;
                EndColor = endColor;
                Start = start;
                End = end;
            }

            public bool IsInsideRange(double value)
            {
                return
                    value >= Start &&
                    value <= End;
            }

            private static double Interpolate(double v0, double v1, double ratio)
            {
                return v0 + ratio * (v1 - v0);
            }

            public HsvColor Interpolate(double totalRatio)
            {
                var totalDistance = End - Start;
                var ratioBetweenPoints = (totalRatio - Start) / totalDistance;

                var forwardHueDistance = StartColor.Hue - EndColor.Hue;
                var backwardHueDistance = EndColor.Hue + (1 - StartColor.Hue);

                double newHue = 0;

                if (forwardHueDistance < backwardHueDistance)
                {
                    newHue = Interpolate(StartColor.Hue, EndColor.Hue, ratioBetweenPoints);
                }
                else
                {
                    newHue = (StartColor.Hue + ratioBetweenPoints*backwardHueDistance) % 1;
                }

                return new HsvColor(
                    hue: newHue,
                    saturation: Interpolate(StartColor.Saturation, EndColor.Saturation, ratioBetweenPoints),
                    value: Interpolate(StartColor.Value, EndColor.Value, ratioBetweenPoints)
                );
            }
        }
    }
}
