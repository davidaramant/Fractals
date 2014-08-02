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
        private readonly string _filename;
        
        private static ILog _log;

        public ComplexNumberListReader(string directory, string filename)
        {
            _directory = directory;
            _filename = filename;
            
            _log = LogManager.GetLogger(GetType());
        }

        public IEnumerable<Complex> GetNumbers()
        {
            var realBytes = new byte[8];
            var imagBytes = new byte[8];

            var fileFormat = String.Format("{0}.*", _filename);
            _log.DebugFormat("Looking in '{0}' for '{1}' files", _directory, fileFormat);

            var files = Directory.GetFiles(_directory, fileFormat);
            _log.DebugFormat("Found {0} files", fileFormat.Length);

            foreach (var file in files)
            {
                _log.DebugFormat("Processing '{0}'", file);
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
