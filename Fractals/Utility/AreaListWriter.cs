using Fractals.Model;
using System;
using System.IO;

namespace Fractals.Utility
{
    public sealed class AreaListWriter
    {
        private readonly string _fullPath;

        public AreaListWriter(string directory, string filename)
        {
            _fullPath = Path.Combine(directory, filename);
        }

        private readonly object _fileLock = new object();

        public void SaveArea(Area area)
        {
            lock (_fileLock)
            {
                using (var stream = new FileStream(_fullPath, FileMode.Append))
                {
                    var realMinBytes = BitConverter.GetBytes(area.RealRange.Minimum);
                    var realMaxBytes = BitConverter.GetBytes(area.RealRange.Maximum);
                    var imagMinBytes = BitConverter.GetBytes(area.ImaginaryRange.Minimum);
                    var imagMaxBytes = BitConverter.GetBytes(area.ImaginaryRange.Maximum);

                    stream.Write(realMinBytes, 0, realMinBytes.Length);
                    stream.Write(realMaxBytes, 0, realMaxBytes.Length);
                    stream.Write(imagMinBytes, 0, imagMinBytes.Length);
                    stream.Write(imagMaxBytes, 0, imagMaxBytes.Length);
                }
            }
        }
    }
}
