using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Fractals.Model;
using Fractals.Utility;

namespace BuddhabrotTrajectoryPlotter
{
    class TrajectoryPlotter
    {
        static void Main(string[] args)
        {
            const string filename = "output.list";

            var list = new ComplexNumberList(filename);

            Console.Out.WriteLine(list.GetNumbers().Count());


            var viewPort = new Area(
                            realRange: new InclusiveRange(-2, 0.5),
                            imagRange: new InclusiveRange(-1.3, 1.3));

            var resolution = new Size(500, 500);

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

            ImageUtility.ColorMatrixToBitmap(output).Save("quick.png");
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
