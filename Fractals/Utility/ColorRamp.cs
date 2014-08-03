using System;
using System.Collections.Generic;
using System.Linq;

namespace Fractals.Utility
{
    public sealed class ColorRamp
    {
        private readonly List<ColorRange> _colorRanges = new List<ColorRange>();

        public ColorRamp(IEnumerable<Tuple<HsvColor, double>> colorPoints)
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

        sealed class ColorRange
        {
            private readonly HsvColor _startColor;
            private readonly HsvColor _endColor;
            private readonly double _start;
            private readonly double _end;

            public ColorRange(HsvColor startColor, HsvColor endColor, double start, double end)
            {
                _startColor = startColor;
                _endColor = endColor;
                _start = start;
                _end = end;
            }

            public bool IsInsideRange(double value)
            {
                return
                    value >= _start &&
                    value <= _end;
            }

            private static double Interpolate(double v0, double v1, double ratio)
            {
                return v0 + ratio * (v1 - v0);
            }

            public HsvColor Interpolate(double totalRatio)
            {
                var totalDistance = _end - _start;
                var ratioBetweenPoints = (totalRatio - _start) / totalDistance;

                var forwardHueDistance = _startColor.Hue - _endColor.Hue;
                var backwardHueDistance = _endColor.Hue + (1 - _startColor.Hue);

                double newHue = 0;

                if (forwardHueDistance < backwardHueDistance)
                {
                    newHue = Interpolate(_startColor.Hue, _endColor.Hue, ratioBetweenPoints);
                }
                else
                {
                    newHue = (_startColor.Hue + ratioBetweenPoints*backwardHueDistance) % 1;
                }

                return new HsvColor(
                    hue: newHue,
                    saturation: Interpolate(_startColor.Saturation, _endColor.Saturation, ratioBetweenPoints),
                    value: Interpolate(_startColor.Value, _endColor.Value, ratioBetweenPoints)
                );
            }
        }
    }
}
