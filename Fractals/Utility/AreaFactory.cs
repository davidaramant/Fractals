using Fractals.Model;

namespace Fractals.Utility
{
    public class AreaFactory
    {
        public static Area SearchArea
        {
            get
            {
                return new Area(
                    new InclusiveRange(-2, 1),
                    new InclusiveRange(-1.5, 1.5));
            }
        }

        public static Area MandelbrotRadiusArea
        {
            get
            {
                return new Area(
                    new InclusiveRange(-2, 2),
                    new InclusiveRange(-2, 2));
            }
        }

        public static Area RenderingArea
        {
            get
            {
                return new Area(
                    new InclusiveRange(-1.75, 1),
                    new InclusiveRange(-1.3, 1.3));
            }
        }
    }
}
