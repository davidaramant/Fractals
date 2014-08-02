using System;
using System.Drawing;
using System.IO;
using Fractals.Model;
using Fractals.Renderer;
using Fractals.Utility;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                args = GetDebuggingArguments();
            }
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            switch (options.Operation)
            {
                case OperationType.RenderMandelbrot:
                    RenderMandelbrot<MandelbrotRenderer>(options);
                    break;
                case OperationType.RenderInterestingPointsMandelbrot:
                    RenderMandelbrot<InterestingPointsRenderer>(options);
                    break;
                case OperationType.FindPoints:
                    FindPoints(options);
                    break;
                case OperationType.PlotPoints:
                    PlotPoints(options);
                    break;
            }
        }

        private static void RenderMandelbrot<T>(Options options)
            where T : MandelbrotRenderer, new()
        {
            var resolution = new Size(options.ResolutionWidth, options.ResolutionHeight);
            var realAxis = new InclusiveRange(-2, 1);
            var imaginaryAxis = new InclusiveRange(-1.5, 1.5);

            var renderer = Activator.CreateInstance<T>();

            Color[,] output = renderer.Render(resolution, realAxis, imaginaryAxis);

            Bitmap image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save(Path.Combine(options.OutputDirectory, String.Format("{0}.bmp", options.Filename)));
        }

        private static void FindPoints(Options options)
        {
            var finder = new PointFinder(options.OutputDirectory, options.Filename);
            finder.Start();

            System.Console.WriteLine("Press <ENTER> to exit.");
            System.Console.ReadLine();
        }

        private static void PlotPoints(Options options)
        {
            var plotter = new Plotter(options.OutputDirectory, options.InputFilename, options.Filename, options.ResolutionWidth, options.ResolutionHeight);
            plotter.Plot();
        }

        private static string[] GetDebuggingArguments()
        {
//            return new[]
//                {
//                    "-t", "RenderMandelbrot",
//                    "-w", "500",
//                    "-h", "500",
//                    "-d", "C:\\temp",
//                    "-f", "mandelbrot"
//                };
//            return new[]
//                {
//                    "-t", "RenderInterestingPointsMandelbrot",
//                    "-w", "500",
//                    "-h", "500",
//                    "-d", "C:\\temp",
//                    "-f", "mandelbrot-areas"
//                };
//                return new[]
//                    {
//                        "-t", "FindPoints",
//                        "-d", "C:\\temp",
//                        "-f", "points"
//                    };
                return new[]
                    {
                        "-t", "PlotPoints",
                        "-w", "500",
                        "-h", "500",
                        "-d", "C:\\temp",
                        "-f", "buddhabrot",
                        "-i", "points"
                    };
        }
    }
}
