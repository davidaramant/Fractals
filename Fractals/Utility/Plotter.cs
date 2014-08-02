using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Fractals.Model;
using log4net;

namespace Fractals.Utility
{
    public class Plotter
    {
        private readonly string _directory ;
        private readonly string _inputFilename;
        private readonly string _filename;
        private readonly int _width;
        private readonly int _height;

        private static ILog _log;

        public Plotter(string directory, string inputFilename, string filename, int width, int height)
        {
            _directory = directory;
            _inputFilename = inputFilename;
            _width = width;
            _filename = filename;
            _height = height;
            
            _log = LogManager.GetLogger(GetType());
        }

        public void Plot()
        {
                var list = new ComplexNumberList(_directory, _inputFilename);
            
                Console.Out.WriteLine(list.GetNumbers().Count());
            
                var viewPort = new Area(
                                realRange: new InclusiveRange(-2, 0.5),
                                imagRange: new InclusiveRange(-1.3, 1.3));
            
                var resolution = new Size(_width, _height);
            
                var plot = new int[resolution.Width, resolution.Height];
            
                var max = 0;
            
                foreach (var number in list.GetNumbers())
                {
                    foreach (var c in GetTrajectory(number))
                    {
                        var point = viewPort.GetPointFromNumber(resolution, c);
            
                        if (point.X < 0 || point.X >= resolution.Width || point.Y < 0 || point.Y >= resolution.Height)
                        {
                            continue;
                        }
            
                        plot[point.X, point.Y]++;
            
                        var temp = plot[point.X, point.Y];
                        if (temp > max)
                        {
                            max = temp;
                        }
                    }
                }
            
                var output = new Color[resolution.Width, resolution.Height];
            
                for (int x = 0; x < resolution.Width; x++)
                {
                    for (int y = 0; y < resolution.Height; y++)
                    {
                        output[x, y] = new HsvColor(0.5, 1, (double)plot[x, y] / max).ToColor();
                    }
                }
            
                ImageUtility.ColorMatrixToBitmap(output).Save(Path.Combine(_directory, String.Format("{0}.png", _filename)));
        }

        static IEnumerable<Complex> GetTrajectory(Complex c)
        {
            Complex z = c;

            for (int i = 0; i < 30000; i++)
            {
                z = z * z + c;

                yield return z;

                if (z.MagnitudeSquared() > 4)
                {
                    yield break;
                }
            }
        }
    }
}