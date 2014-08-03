using CommandLine;
using CommandLine.Text;

namespace Console
{
    public class Options
    {
        [Option('t', "type", Required = true, HelpText = "The type of operation to perform")]
        public OperationType Operation { get; set; }

        [Option('h', "height", Required = false, HelpText = "The resolution height (in pixels)")]
        public int ResolutionHeight { get; set; }

        [Option('w', "width", Required = false, HelpText = "The resolution width (in pixels)")]
        public int ResolutionWidth { get; set; }

        [Option('d', "directory", Required = true, HelpText = "The output directory")]
        public string OutputDirectory { get; set; }

        [Option('f', "filename", Required = true, HelpText = "The output filename")]
        public string Filename { get; set; }

        [Option('i', "input-directory", Required = false, HelpText = "The input directory")]
        public string InputDirectory { get; set; }

        [Option('p', "input-pattern", Required = false, HelpText = "The input filename pattern")]
        public string InputFilenamePattern { get; set; }

        [Option('n', "minimum", Required = false, HelpText = "The minimum")]
        public int Miniumum { get; set; }

        [Option('x', "maximum", Required = false, HelpText = "The maxiumum")]
        public int Maxiumum { get; set; }

        [Option('s', "strategy", Required = false, HelpText = "The point finding strategy")]
        public PointStrategy Strategy { get; set; }

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
        RenderMandelbrotDistance,
        RenderInterestingPointsMandelbrot,
        FindPoints,
        PlotPoints,
        PlotAntiBuddhabrot, 
        RenderPoints,
        RenderMandelbrotEscapeFancy,
    }

    public enum PointStrategy
    {
        CompletelyRandom,
        ExcludeBulbs,
        AreasAndBulbExclusion
    }
}