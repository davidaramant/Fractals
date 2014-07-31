using Fractals.Model;
using System;
using System.Security.Cryptography;

namespace Fractals.Utility
{
    public sealed class ReallyRandom
    {
        private readonly RNGCryptoServiceProvider _rng = new RNGCryptoServiceProvider();
        private const int _bufferSize = sizeof(UInt64) * 128;
        private readonly byte[] _randomBuffer = new byte[_bufferSize];

        private int _bufferOffset = _bufferSize;

        private void FillBuffer()
        {
            _rng.GetBytes(_randomBuffer);
            _bufferOffset = 0;
        }

        public double Next()
        {
            if (_bufferOffset >= _randomBuffer.Length)
            {
                FillBuffer();
            }
            var value = BitConverter.ToUInt64(_randomBuffer, _bufferOffset);
            _bufferOffset += sizeof(UInt64);
            return (double)value / UInt64.MaxValue;
        }

        public double Next(InclusiveRange range)
        {
            return range.Magnitude * Next() + range.Min;
        }
    }
}
