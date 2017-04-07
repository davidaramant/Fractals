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
                    RenderPlot(options);
                    break;
                case OperationType.RenderNebulaPlots:
                    RenderNebulabrot(options);
                    break;
                case OperationType.FindEdgeAreas:
                    FindEdgeAreas(options);
                    break;
                case OperationType.RenderPoints:
                    RenderPoints(options);
                    break;
                case OperationType.RenderSpectrumPlot:
                    RenderSpectrumPlot(options);
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
            var serializer = new XmlSerializer(typeof(T));

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                return (T)serializer.Deserialize(stream);
            }
        }

        private void RenderMandelbrot<T>(Options options)
            where T : IGenerator, new()
        {
            var arguments = DeserializeArguments<ExampleImageRendererArguments>(options.ConfigurationFilepath);
            var viewPort = AreaFactory.MandelbrotRadiusArea;

            var renderer = Activator.CreateInstance<T>();

            Color[,] output = renderer.Render(arguments.Resolution.ToSize(), viewPort);

            using (var image = ImageUtility.ColorMatrixToBitmap(output))
            {
                image.Save(Path.Combine(arguments.OutputDirectory, $"{arguments.OutputFilename}.png"));
            }
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
                case PointSelectionStrategy.BulbsOnly:
                    generator = new BulbsOnlyPointGenerator();
                    break;
                case PointSelectionStrategy.EdgesAndBulbsOnly:
                    generator = new EdgeAreasAndBulbsPointGenerator(arguments.InputDirectory, arguments.InputEdgeFilename);
                    break;
                default:
                    throw new ArgumentException();
            }

            PointFinder finder;
            switch (arguments.SelectionStrategy)
            {
                case PointSelectionStrategy.Random:
                case PointSelectionStrategy.BulbsExcluded:
                case PointSelectionStrategy.EdgesWithBulbsExcluded:
                    finder = new BuddhabrotPointFinder(arguments.MinimumThreshold, arguments.MaximumThreshold, arguments.OutputDirectory, arguments.OutputFilenamePrefix, generator);
                    break;
                case PointSelectionStrategy.BulbsOnly:
                case PointSelectionStrategy.EdgesAndBulbsOnly:
                    finder = new MandelbrotPointFinder(arguments.MinimumThreshold, arguments.MaximumThreshold, arguments.OutputDirectory, arguments.OutputFilenamePrefix, generator);
                    break;
                default:
                    throw new ArgumentException();
            }

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
            var plotter = new TrajectoryPlotter(arguments.InputDirectory, arguments.InputFilePattern, arguments.OutputDirectory, arguments.OutputFilename, arguments.Resolution.Width, arguments.Resolution.Height, arguments.Bailout);
            plotter.Plot();
        }

        private void RenderPlot(Options options)
        {
            var arguments = DeserializeArguments<RenderingArguments>(options.ConfigurationFilepath);
            var renderer = new PlotRenderer(
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

        private void RenderPoints(Options options)
        {
            var arguments = DeserializeArguments<PointRenderingArguments>(options.ConfigurationFilepath);
            var renderer = new PointRenderer(arguments.InputDirectory, arguments.InputFilePattern, arguments.Resolution.Width, arguments.Resolution.Height);
            renderer.Render(arguments.OutputDirectory, arguments.OutputFilename);
        }

        private void RenderSpectrumPlot(Options options)
        {
            throw new NotSupportedException("What the hell is spectrum plot");
            var arguments = DeserializeArguments<SpectrumRenderingArguments>(options.ConfigurationFilepath);
            var renderer = new SpectrumPlotRenderer(arguments.InputDirectory, arguments.InputFilename, arguments.Resolution.Width, arguments.Resolution.Height);
            renderer.Render(arguments.OutputDirectory, arguments.OutputFilenamePrefix, arguments.StartingColor, arguments.EndingColor, arguments.StepSize);
        }

        private static void SetupParallelismLimits(Options options)
        {
            var processorCount = Environment.ProcessorCount;
            if (options.Utilization.HasValue)
            {
                GlobalArguments.DegreesOfParallelism = options.Utilization.Value;

                _log.InfoFormat("Parallel operations set to use {0} of {1} processors", options.Utilization.Value, processorCount);
            }
            else
            {
                GlobalArguments.DegreesOfParallelism = processorCount;

                _log.DebugFormat("Parallel options set to use all processors available");
            }
        }

        private string[] GetDebuggingArguments()
        {
            /*--------------------------------------------------------------------------------------
            |  The following items are used to render explanatory images.                          |
            --------------------------------------------------------------------------------------*/

            //return new[] { "-t", "RenderMandelbrot", "-c", @"..\..\..\..\Argument Files\RenderMandelbrot.xml" };
            //return new[] { "-t", "RenderMandelbrotEscapePlain", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotEscapePlain.xml" };
            //return new[] { "-t", "RenderMandelbrotEscapeFancy", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotEscapeFancy.xml" };
            //return new[] { "-t", "RenderMandelbrotDistance", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotDistance.xml" };
            //return new[] { "-t", "RenderMandelbrotEdges", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotEdges.xml" };

            /*--------------------------------------------------------------------------------------
            |  The following set of operations (done in order) generates a buddhabrot image.       |
            --------------------------------------------------------------------------------------*/

            //return new[] { "-t", "FindEdgeAreas", "-c", @"..\..\..\..\Argument Files\EdgeAreas.xml" };
            //return new[] { "-t", "FindPoints", "-c", @"..\..\..\..\Argument Files\FindBuddhabrotPoints.xml" };
            //return new[] { "-t", "PlotPoints", "-c", @"..\..\..\..\Argument Files\PlotBuddhabrotPoints.xml" };
            //return new[] { "-t", "RenderPlot", "-c", @"..\..\..\..\Argument Files\RenderBuddhabrotPlot.xml" };

            /*--------------------------------------------------------------------------------------
            |  The following set of operations (done in order) generates an anti-buddhabrot image. |
            --------------------------------------------------------------------------------------*/

            //return new[] { "-t", "FindEdgeAreas", "-c", @"..\..\..\..\Argument Files\EdgeAreas.xml" };
            //return new[] { "-t", "FindPoints", "-c", @"..\..\..\..\Argument Files\FindMandelbrotPoints.xml" };
            //return new[] { "-t", "PlotPoints", "-c", @"..\..\..\..\Argument Files\PlotMandelbrotPoints.xml" };
            //return new[] { "-t", "RenderPlot", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotPlot.xml" };

            /*--------------------------------------------------------------------------------------
            |  The following set of operations (done in order) generates a mandelbrot image.       |
            --------------------------------------------------------------------------------------*/

            //return new[] { "-t", "FindEdgeAreas", "-c", @"..\..\..\..\Argument Files\EdgeAreas.xml" };
            //return new[] { "-t", "FindPoints", "-c", @"..\..\..\..\Argument Files\FindMandelbrotPoints.xml" };
            //return new[] { "-t", "RenderPoints", "-c", @"..\..\..\..\Argument Files\RenderMandelbrotPoints.xml" };

            /*--------------------------------------------------------------------------------------
            |  After generating three separate plots (with different maximum bailout values), the  |
            |  following operation generates an anti-buddhabrot image.                             |
            |                                                                                      |
            |  Note: The image is cleaner if the random strategy is used to gather the points.     |
            --------------------------------------------------------------------------------------*/

            //return new[] { "-t", "RenderNebulaPlots", "-c", @"..\..\..\..\Argument Files\RenderNebulaPlot.xml" };

            /*--------------------------------------------------------------------------------------
            |  The following set of operations (done in order) generates a set of buddhabrot       |
            |  images spanning a color spectrum.                                                   |
            --------------------------------------------------------------------------------------*/

            //return new[] { "-t", "FindEdgeAreas", "-c", @"..\..\..\..\Argument Files\EdgeAreas.xml" };
            //return new[] { "-t", "FindPoints", "-c", @"..\..\..\..\Argument Files\FindBuddhabrotPoints.xml" };
            //return new[] { "-t", "PlotPoints", "-c", @"..\..\..\..\Argument Files\PlotBuddhabrotPoints.xml" };
            //return new[] { "-t", "RenderSpectrumPlot", "-c", @"..\..\..\..\Argument Files\RenderBuddhabrotPointsSpectrum.xml" };

            return new string[0];
        }
    }
}
