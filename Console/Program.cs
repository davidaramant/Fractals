using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Fractals.Arguments;
using Fractals.PointGenerator;
using Fractals.Renderer;
using Fractals.Utility;
using log4net;

namespace Fractals.Console
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

            SetupParallelismLimits(options);

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
                case OperationType.RenderMandelbrotEdges:
                    RenderMandelbrot<EdgeAreasRenderer>(options);
                    break;
                case OperationType.FindPoints:
                    FindPoints(options);
                    break;
                case OperationType.PlotPoints:
                    PlotPoints(options);
                    break;
                case OperationType.RenderPlot:
                    RenderPoints(options);
                    break;
                case OperationType.RenderNebulaPlots:
                    RenderNebulabrot(options);
                    break;
                case OperationType.FindEdgeAreas:
                    FindEdgeAreas(options);
                    break;
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Console.WriteLine("DONE at {0}! Press <ENTER> to exit...", DateTime.Now);
                System.Console.ReadLine();
            }
        }

        private static T DeserializeArguments<T>(string path)
        {
            var serializer = new XmlSerializer(typeof (T));

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return (T) serializer.Deserialize(stream);
            }
        }

        private void RenderMandelbrot<T>(Options options)
            where T : IGenerator, new()
        {
            var arguments = DeserializeArguments<ExampleImageRendererArguments>(options.ConfigurationFilepath);
            var viewPort = AreaFactory.SearchArea;

            var renderer = Activator.CreateInstance<T>();

            Color[,] output = renderer.Render(arguments.Resolution.ToSize(), viewPort);

            Bitmap image = ImageUtility.ColorMatrixToBitmap(output);

            image.Save(Path.Combine(arguments.OutputDirectory, String.Format("{0}.png", arguments.OutputFilename)));
        }

        private void FindPoints(Options options)
        {
            var arguments = DeserializeArguments<PointFinderArguments>(options.ConfigurationFilepath);

            RandomPointGenerator generator;
            switch (arguments.SelectionStrategy)
            {
                case PointSelectionStrategy.Random:
                    generator = new RandomPointGenerator();
                    break;
                case PointSelectionStrategy.BulbsExcluded:
                    generator = new BulbsExcludedPointGenerator();
                    break;
                case PointSelectionStrategy.EdgesWithBulbsExcluded:
                    generator = new EdgeAreasWithBulbsExcludedPointGenerator(arguments.InputDirectory, arguments.InputEdgeFilename);
                    break;
                default:
                    throw new ArgumentException();
            }
            var finder = new PointFinder(arguments.MinimumThreshold, arguments.MaximumThreshold, arguments.OutputDirectory, arguments.OutputFilenamePrefix, generator);

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
            var arguments = DeserializeArguments<PointPlottingArguments>(options.ConfigurationFilepath);
            var plotter = new Plotter(arguments.InputDirectory, arguments.InputFilePattern, arguments.OutputDirectory, arguments.OutputFilename, arguments.Resolution.Width, arguments.Resolution.Height);
            plotter.Plot();
        }

        private void RenderPoints(Options options)
        {
            var arguments = DeserializeArguments<RenderingArguments>(options.ConfigurationFilepath);
            var renderer = new PointRenderer(
                inputDirectory: arguments.InputDirectory,
                inputFilename: arguments.InputFilename,
                width: arguments.Resolution.Width,
                height: arguments.Resolution.Height);

            renderer.Render(outputDirectory: arguments.OutputDirectory, outputFilename: arguments.OutputFilename);
        }

        private void RenderNebulabrot(Options options)
        {
            var arguments = DeserializeArguments<NebulaRenderingArguments>(options.ConfigurationFilepath);
            var renderer = new NebulaPointRenderer(
                inputDirectory: arguments.InputDirectory,
                inputFilenameRed: arguments.RedInputFilename,
                inputFilenameGreen: arguments.GreenInputFilename,
                inputFilenameBlue: arguments.BlueInputFilename,
                width: arguments.Resolution.Width,
                height: arguments.Resolution.Height);

            renderer.Render(outputDirectory: arguments.OutputDirectory, outputFilename: arguments.OutputFilename);
        }

        private void FindEdgeAreas(Options options)
        {
            var arguments = DeserializeArguments<EdgeAreaArguments>(options.ConfigurationFilepath);
            var locator = new EdgeLocator(arguments.OutputDirectory, arguments.OutputFilename);
            locator.StoreEdges(arguments.Resolution.ToSize(), arguments.GridSize, AreaFactory.SearchArea);
        }

        private static void SetupParallelismLimits(Options options)
        {
            var processorCount = Environment.ProcessorCount;
            if (options.Utilization.HasValue)
            {
                var numberToUse = processorCount * options.Utilization.Value / 100;
                if (numberToUse == 0)
                {
                    numberToUse = 1;
                }

                GlobalArguments.DegreesOfParallelism = numberToUse;

                _log.InfoFormat("Parallel operations set to use {0} of {1} processors", numberToUse, processorCount);
            }
            else
            {
                GlobalArguments.DegreesOfParallelism = processorCount;

                _log.DebugFormat("Parallel options set to use all processors available");
            }
        }

        private string[] GetDebuggingArguments()
        {
            //return new[] { "-t", "RenderMandelbrot", "-c", @"..\..\..\..\Argument Files\RenderMandelbrot.xml" };
            //return new[] { "-t", "RenderMandelbrotEscapePlain", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotEscapePlain.xml" };
            //return new[] { "-t", "RenderMandelbrotEscapeFancy", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotEscapeFancy.xml" };
            //return new[] { "-t", "RenderMandelbrotDistance", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotDistance.xml" };
            //return new[] { "-t", "RenderMandelbrotEdges", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotEdges.xml" };
            //return new[] { "-t", "FindPoints", "-c", @"..\..\..\..\Argument Files\FindBuddhabrotPoints.xml" };
            //return new[] { "-t", "FindPoints", "-c", @"..\..\..\..\Argument Files\FindAntiBuddhabrotPoints.xml" };
            //return new[] { "-t", "PlotPoints", "-c", @"..\..\..\..\Argument Files\PlotPoints.xml" };
            //return new[] { "-t", "RenderPlot", "-c", @"..\..\..\..\Argument Files\RenderPlot.xml" };
            //return new[] { "-t", "RenderNebulaPlots", "-c", @"..\..\..\..\Argument Files\RenderNebulaPlot.xml" };
            //return new[] { "-t", "RenderMandelbrot", "-c", @"..\..\..\..\Argument Files\RenderMandelbrot.xml" };
            //return new[] { "-t", "RenderMandelbrot", "-c", @"..\..\..\..\Argument Files\RenderMandelbrot.xml" };
            //return new[] { "-t", "FindEdgeAreas", "-c", @"..\..\..\..\Argument Files\EdgeAreas.xml" };
            return new string[0];
        }
    }
}
