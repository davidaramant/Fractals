using Fractals.Model;
using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace Fractals.Utility
{
    public sealed class ComplexNumberListReader
    {
        private readonly string _directory;
        private readonly string _filenamePattern;
        
        private static ILog _log;

        public ComplexNumberListReader(string directory, string filenamePattern)
        {
            _directory = directory;
            _filenamePattern = filenamePattern;
            
            _log = LogManager.GetLogger(GetType());
        }

        public IEnumerable<Complex> GetNumbers()
        {
            var realBytes = new byte[8];
            var imagBytes = new byte[8];

            _log.DebugFormat("Looking in '{0}' for '{1}' files", _directory, _filenamePattern);

            var files = Directory.GetFiles(_directory, _filenamePattern);
            _log.DebugFormat("Found {0:N0} files", files.Length);

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                _log.DebugFormat("Processing '{0}'", fileInfo.Name);
                using (var stream = File.OpenRead(file))
                {
                    while (true)
                    {
                        if (stream.Read(realBytes, 0, 8) != 8)
                        {
                            break;
                        }
                        if (stream.Read(imagBytes, 0, 8) != 8)
                        {
                            break;
                        }

                        yield return new Complex(
                            BitConverter.ToDouble(realBytes, 0),
                            BitConverter.ToDouble(imagBytes, 0));
                    }
                }
            }

        }
    }
}
