using Fractals.Model;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fractals.Utility
{
    public sealed class ComplexNumberList
    {
        private readonly string _filePath;

        public ComplexNumberList(string filePath)
        {
            _filePath = filePath;
        }

        private readonly object _fileLock = new object();

        public void SaveNumber(Complex number)
        {
            lock (_fileLock)
            {
                using (var stream = new FileStream(_filePath,FileMode.Append))
                {
                    var realBytes = BitConverter.GetBytes(number.Real);
                    var imagBytes = BitConverter.GetBytes(number.Imag);

                    stream.Write(realBytes, 0, realBytes.Length);
                    stream.Write(imagBytes, 0, imagBytes.Length);
                }
            }
        }

        public IEnumerable<Complex> GetNumbers()
        {
            var realBytes = new byte[8];
            var imagBytes = new byte[8];

            using (var stream = File.OpenRead(_filePath))
            {
                while (true)
                {
                    if (stream.Read(realBytes, 0, 8) != 8)
                    {
                        yield break;
                    }
                    if (stream.Read(imagBytes, 0, 8) != 8)
                    {
                        yield break;
                    }

                    yield return new Complex(
                        BitConverter.ToDouble(realBytes, 0),
                        BitConverter.ToDouble(imagBytes, 0));
                }
            }
        }
    }
}
