using System.Linq;
using System.Text;

namespace Benchmarks
{
    public sealed class Fp128
    {
        private readonly uint[] _buffer = new uint[4];

        public Fp128(int integer)
        {
            if (integer >= 0)
            {
                _buffer[0] = (uint)integer;
            }
            else
            {
                unchecked
                {
                    _buffer[0] = ((uint)integer) - 1;
                }
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            var negMask = (uint)1 << 31;

            if ((_buffer[0] & negMask) == negMask)
            {
                sb.Append("-");
                sb.Append(~_buffer[0]);
            }
            else
            {
                sb.Append(_buffer[0]);
            }

            return sb.ToString();
        }

        public uint[] GetBufferCopy() => _buffer.ToArray();

        public string GetPrettyPrintHex() => string.Join(" ", _buffer.Select(u => u.ToString("X8")));
    }
}
