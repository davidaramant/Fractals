using Fractals.Model;
using System;
using System.IO;
using log4net;

namespace Fractals.Utility
{
    public sealed class ComplexNumberListWriter
    {
        private readonly string _directory;
        private readonly string _filename;
        private string _currentFilename;

        private int _count;
        private int _fileNumber;

        private const int MaxCountPerFile = 64 * 1024 * 100;
        
        private static ILog _log;

        public ComplexNumberListWriter(string directory, string filename)
        {
            _directory = directory;
            _filename = filename;
            
            _log = LogManager.GetLogger(GetType());

            ChangeFilename();
        }

        private readonly object _fileLock = new object();

        public void SaveNumber(Complex number)
        {
            lock (_fileLock)
            {
                _count ++;

                if (_count%MaxCountPerFile == 0)
                {
                    _fileNumber++;
                    ChangeFilename();
                }

                using (var stream = new FileStream(_currentFilename, FileMode.Append))
                {
                    var realBytes = BitConverter.GetBytes(number.Real);
                    var imagBytes = BitConverter.GetBytes(number.Imag);

                    stream.Write(realBytes, 0, realBytes.Length);
                    stream.Write(imagBytes, 0, imagBytes.Length);
                }
            }
        }

        private void ChangeFilename()
        {
            var newFilename = String.Format("{0}.{1}", _filename, _fileNumber);
            _currentFilename = Path.Combine(_directory, newFilename);

            _log.InfoFormat("Swithing file to: {0}", newFilename);
        }
    }
}
