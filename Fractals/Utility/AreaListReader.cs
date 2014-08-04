using Fractals.Model;
using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace Fractals.Utility
{
    public sealed class AreaListReader
    {
        private readonly string _filename;
        private readonly string _fullPath;
        
        private static ILog _log;

        public AreaListReader(string directory, string filename)
        {
            _fullPath = Path.Combine(directory, filename);
            _filename = filename;
            
            _log = LogManager.GetLogger(GetType());
        }

        public IEnumerable<Area> GetAreas()
        {
            var realMinBytes = new byte[8];
            var realMaxBytes = new byte[8];
            var imagMinBytes = new byte[8];
            var imagMaxBytes = new byte[8];

            _log.DebugFormat("Reading from {0}", _filename);

            using (var stream = new FileStream(_fullPath, FileMode.Open, FileAccess.Read))
            {
                while (true)
                {
                    if (stream.Read(realMinBytes, 0, 8) != 8)
                    {
                        break;
                    }
                    if (stream.Read(realMaxBytes, 0, 8) != 8)
                    {
                        break;
                    }
                    if (stream.Read(imagMinBytes, 0, 8) != 8)
                    {
                        break;
                    }
                    if (stream.Read(imagMaxBytes, 0, 8) != 8)
                    {
                        break;
                    }

                    yield return new Area(
                        new InclusiveRange(
                            BitConverter.ToDouble(realMinBytes, 0),
                            BitConverter.ToDouble(realMaxBytes, 0)),
                        new InclusiveRange(
                            BitConverter.ToDouble(imagMinBytes, 0),
                            BitConverter.ToDouble(imagMaxBytes, 0)));
                }
            }
        }
    }
}
