using Fractals.Model;

namespace Fractals.Utility
{
    public class AreaFactory
    {
        public static Area SearchArea => new Area(
            new InclusiveRange(-2, 1),
            new InclusiveRange(-1.5, 1.5));

        public static Area MandelbrotRadiusArea => new Area(
            new InclusiveRange(-2, 2),
            new InclusiveRange(-2, 2));

        public static Area RenderingArea => new Area(
            new InclusiveRange(-1.75, 1),
            new InclusiveRange(-1.3, 1.3));
    }
}
