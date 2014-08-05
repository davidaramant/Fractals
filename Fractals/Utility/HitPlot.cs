using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Fractals.Utility
{
    public sealed class HitPlot : IHitPlot
    {
        private readonly int _fourthWidth;
        private readonly int _fourthHeight;

        private readonly int[] _hits00;
        private readonly int[] _hits10;
        private readonly int[] _hits20;
        private readonly int[] _hits30;

        private readonly int[] _hits01;
        private readonly int[] _hits11;
        private readonly int[] _hits21;
        private readonly int[] _hits31;

        private readonly int[] _hits02;
        private readonly int[] _hits12;
        private readonly int[] _hits22;
        private readonly int[] _hits32;

        private readonly int[] _hits03;
        private readonly int[] _hits13;
        private readonly int[] _hits23;
        private readonly int[] _hits33;

        public HitPlot(Size resolution)
        {
            _fourthWidth = resolution.Width / 4;
            _fourthHeight = resolution.Height / 4;

            int quadrantSize = _fourthWidth * _fourthHeight;

            _hits00 = new int[quadrantSize];
            _hits10 = new int[quadrantSize];
            _hits20 = new int[quadrantSize];
            _hits30 = new int[quadrantSize];

            _hits01 = new int[quadrantSize];
            _hits11 = new int[quadrantSize];
            _hits21 = new int[quadrantSize];
            _hits31 = new int[quadrantSize];

            _hits02 = new int[quadrantSize];
            _hits12 = new int[quadrantSize];
            _hits22 = new int[quadrantSize];
            _hits32 = new int[quadrantSize];

            _hits03 = new int[quadrantSize];
            _hits13 = new int[quadrantSize];
            _hits23 = new int[quadrantSize];
            _hits33 = new int[quadrantSize];
        }

        public void SaveTrajectories(string filePath)
        {
            using (var file = new FileStream(filePath, FileMode.Create))
            {
                // Row 0
                foreach (var i in _hits00)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits10)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits20)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits30)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }

                // Row 1
                foreach (var i in _hits01)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits11)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits21)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits31)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }

                // Row 2
                foreach (var i in _hits02)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits12)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits22)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits32)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }

                // Row 3
                foreach (var i in _hits03)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits13)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits23)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                foreach (var i in _hits33)
                {
                    var buffer = BitConverter.GetBytes(i);
                    file.Write(buffer, 0, buffer.Count());
                }
                file.Flush();
                file.Close();
            }
        }

        public void LoadTrajectories(string filePath)
        {
            using (var file = File.OpenRead(filePath))
            {
                const int intSize = sizeof(Int32);
                var buffer = new byte[sizeof(Int32)];
                var tempInt = 0;

                // Row 0
                for(int i = 0; i < _hits00.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits00[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits10.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits10[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits20.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits20[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits30.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits30[i] = BitConverter.ToInt32(buffer, 0);
                }

                // Row 1
                for (int i = 0; i < _hits01.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits01[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits11.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits11[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits21.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits21[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits31.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits31[i] = BitConverter.ToInt32(buffer, 0);
                }

                // Row 2
                for (int i = 0; i < _hits02.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits02[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits12.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits12[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits22.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits22[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits32.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits32[i] = BitConverter.ToInt32(buffer, 0);
                }

                // Row 3
                for (int i = 0; i < _hits03.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits03[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits13.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits13[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits23.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits23[i] = BitConverter.ToInt32(buffer, 0);
                }
                for (int i = 0; i < _hits33.Length; i++)
                {
                    file.Read(buffer, 0, intSize);
                    _hits33[i] = BitConverter.ToInt32(buffer, 0);
                }
            }
        }

        private int[] GetSegment(int x, int y)
        {
            var xQuadrant = x / _fourthWidth;
            var yQuadrant = y / _fourthHeight;

            // Handle points that fall exactly on the edge
            if (xQuadrant == 4)
                xQuadrant--;
            if (yQuadrant == 4)
                yQuadrant--;

            switch (xQuadrant)
            {
                case 0:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits00;
                        case 1:
                            return _hits01;
                        case 2:
                            return _hits02;
                        case 3:
                            return _hits03;

                        default:
                            throw new Exception("NO WAY");
                    }

                case 1:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits10;
                        case 1:
                            return _hits11;
                        case 2:
                            return _hits12;
                        case 3:
                            return _hits13;

                        default:
                            throw new Exception("NO WAY");
                    }

                case 2:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits20;
                        case 1:
                            return _hits21;
                        case 2:
                            return _hits22;
                        case 3:
                            return _hits23;

                        default:
                            throw new Exception("NO WAY");
                    }

                case 3:
                    switch (yQuadrant)
                    {
                        case 0:
                            return _hits30;
                        case 1:
                            return _hits31;
                        case 2:
                            return _hits32;
                        case 3:
                            return _hits33;

                        default:
                            throw new Exception("NO WAY");
                    }

                default:
                    throw new Exception("NO WAY");
            }
        }

        public void IncrementPoint(Point p)
        {
            var segment = GetSegment(p.X, p.Y);

            var offset = (p.X % _fourthWidth) + (_fourthWidth * (p.Y % _fourthHeight));

            Interlocked.Increment(ref segment[offset]);
        }

        public int GetHitsForPoint(Point p)
        {
            var segment = GetSegment(p.X, p.Y);

            var offset = (p.X % _fourthWidth) + (_fourthWidth * (p.Y % _fourthHeight));

            return segment[offset];
        }

        public int FindMaximumHit()
        {
            return new[]
            {
                _hits00.Max(),
                _hits10.Max(),
                _hits20.Max(),
                _hits30.Max(),

                _hits01.Max(),
                _hits11.Max(),
                _hits21.Max(),
                _hits31.Max(),

                _hits02.Max(),
                _hits12.Max(),
                _hits22.Max(),
                _hits32.Max(),

                _hits03.Max(),
                _hits13.Max(),
                _hits23.Max(),
                _hits33.Max(),

            }.Max();
        }
    }
}
