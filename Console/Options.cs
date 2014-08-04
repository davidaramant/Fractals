using CommandLine;
using CommandLine.Text;

namespace Fractals.Console
{
    public class Options
    {
        [Option('t', "type", Required = true, HelpText = "The type of operation to perform")]
        public OperationType Operation { get; set; }

        [Option('c', "config", Required = true, HelpText = "The path to the configuration file")]
        public string ConfigurationFilepath { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

    public enum OperationType
    {
        RenderMandelbrot,
        RenderMandelbrotEscapePlain,
        RenderMandelbrotEscapeFancy,
        RenderMandelbrotDistance,
        RenderMandelbrotEdges,
        FindPoints,
        PlotPoints,
        RenderPlot,
        RenderNebulaPlots,
    }
}