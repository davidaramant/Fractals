using Fractals.Model;
using System;
using System.Security.Cryptography;

namespace Fractals.Utility
{
    public sealed class CryptoRandom
    {
        private readonly RNGCryptoServiceProvider _cryptoServiceProvider = new RNGCryptoServiceProvider();
        private const int BufferSize = sizeof(UInt64) * 128;
        private readonly byte[] _randomBuffer = new byte[BufferSize];

        private int _bufferOffset = BufferSize;

        private void FillBuffer()
        {
            _cryptoServiceProvider.GetBytes(_randomBuffer);
            _bufferOffset = 0;
        }

        public double NextDouble()
        {
            if (_bufferOffset >= _randomBuffer.Length)
            {
                FillBuffer();
            }
            var value = BitConverter.ToUInt64(_randomBuffer, _bufferOffset) / (1 << 11);
            _bufferOffset += sizeof(UInt64);
            return (double) value / (1UL << 53);
        }

        public double Next(InclusiveRange range)
        {
            return range.Magnitude * NextDouble() + range.Minimum;
        }
    }
}
