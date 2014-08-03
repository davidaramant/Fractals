using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Fractals.Model;
using Fractals.Renderer;
using Fractals.Utility;
using log4net;

namespace Console
{
    class Program
    {
        private static ILog _log;

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            new Program().Process(args);
        }

        public Program()
        {
            _log = LogManager.GetLogger(GetType());
        }

        private void Process(string[] args)
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

            _log.InfoFormat("Operation: {0}", options.Operation);

            switch (options.Operation)
            {
                case OperationType.RenderMandelbrot:
                    RenderMandelbrot<MandelbrotRenderer>(options);
                    break;
                case OperationType.RenderMandelbrotEscapePlain:
                    RenderMandelbrot<MandelbrotEscapeRenderer>(options);
                    break;
                case OperationType.RenderMandelbrotEscapeFancy:
                    RenderMandelbrot<MandelbrotEscapeRendererFancy>(options);
                    break;
                case OperationType.RenderMandelbrotDistance:
                    RenderMandelbrot<MandelbrotDistanceRenderer>(options);
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
                case OperationType.PlotAntiBuddhabrot:
                    PlotAntiBuddhabrot(options);
                    break;
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Console.WriteLine("DONE at {0}! Press <ENTER> to exit...", DateTime.Now);
                System.Console.ReadLine();
            }
        }

        private void RenderMandelbrot<T>(Options options)
            where T : IGenerator, new()
        {
            var resolution = new Size(options.ResolutionWidth, options.ResolutionHeight);
            var realAxis = new InclusiveRange(-2, 1);
            var imaginaryAxis = new InclusiveRange(-1.5, 1.5);

            var renderer = Activator.CreateInstance<T>();

            Color[,] output = renderer.Render(resolution, realAxis, imaginaryAxis);

            Bitmap image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save(Path.Combine(options.OutputDirectory, String.Format("{0}.png", options.Filename)));
        }

        private void FindPoints(Options options)
        {
            RandomPointGenerator generator;
            switch (options.Strategy)
            {
                case PointStrategy.CompletelyRandom:
                    generator = new RandomPointGenerator();
                    break;
                case PointStrategy.ExcludeBulbs:
                    generator = new ExcludingBulbPointGenerator();
                    break;
                case PointStrategy.AreasAndBulbExclusion:
                    generator = new InterestingAreasPointGenerator();
                    break;
                default:
                    throw new ArgumentException();
            }
            var finder = new BuddhabrotNumberFinder(options.Miniumum, options.Maxiumum, options.OutputDirectory, options.Filename, generator);

            System.Console.WriteLine("Press <ENTER> to stop...");

            Task.Factory.StartNew(() =>
            {
                System.Console.ReadLine();
                finder.Stop();
            });

            finder.Start();
        }

        private void PlotPoints(Options options)
        {
            var plotter = new FilePlotter(options.InputDirectory, options.InputFilenamePattern, options.OutputDirectory, options.Filename, options.ResolutionWidth, options.ResolutionHeight);
            plotter.Plot();
        }

        private void PlotAntiBuddhabrot(Options options)
        {
            var plotter = new AntiBuddhabrotPlotter(options.OutputDirectory, options.Filename, options.ResolutionWidth, options.ResolutionHeight);
            plotter.Plot();
        }

        private string[] GetDebuggingArguments()
        {
            //return new[]
            //    {
            //        "-t", "RenderMandelbrot",
            //        "-w", "1024",
            //        "-h", "1024",
            //        "-d", @"C:\temp\out",
            //        "-f", "mandelbrot"
            //    };
            //return new[]
            //    {
            //        "-t", "RenderMandelbrotEscapePlain",
            //        "-w", "2048",
            //        "-h", "2048",
            //        "-d", @"C:\temp\out",
            //        "-f", "mandelbrot_escape"
            //    };
            //return new[]
            //    {
            //        "-t", "RenderMandelbrotEscapeFancy",
            //        "-w", "1024",
            //        "-h", "1024",
            //        "-d", @"C:\temp\out",
            //        "-f", "mandelbrot_escape_fancy"
            //    }; 
            //return new[]
            //    {
            //        "-t", "RenderMandelbrotDistance",
            //        "-w", "1024",
            //        "-h", "1024",
            //        "-d", @"C:\temp\out",
            //        "-f", "mandelbrot_distance"
            //    };
            //return new[]
            //    {
            //        "-t", "RenderInterestingPointsMandelbrot",
            //        "-w", "1024",
            //        "-h", "1024",
            //        "-d", @"C:\temp\out",
            //        "-f", "mandelbrot-areas"
            //    };
            //return new[]
            //    {
            //        "-t", "FindPoints",
            //        "-d", @"C:\temp\out",
            //        "-f", "points",
            //        "-n", "20000",
            //        "-x", "30000",
            //        "-s", "AreasAndBulbExclusion"
            //    };
            return new[]
                {
                    "-t", "PlotPoints",
                    "-w", "22400",
                    "-h", "22400",
                    "-d", @"C:\temp\out",
                    "-f", "buddhabrot",
                    "-i", @"C:\temp\in",
                    "-p", "points*"
                };
            //return new[]
            //    {
            //        "-t", "PlotAntiBuddhabrot",
            //        "-d", @"C:\temp\out",
            //        "-f", "anti-buddhabrot",
            //        "-w", "2048",
            //        "-h", "2048"
            //    };
        }
    }
}
