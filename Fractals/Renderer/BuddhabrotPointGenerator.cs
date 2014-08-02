using Fractals.Model;

namespace Fractals.Renderer
{
    public static class BuddhabrotPointGenerator
    {
        public static bool IsPointInBuddhabrot(Complex c, BailoutRange bailoutRange)
        {
            Complex z = c;

            for (int i = 0; i < bailoutRange.Max; i++)
            {
                z = z * z + c;

                if (z.MagnitudeSquared() > 4)
                {
                    return bailoutRange.IsInside(i);
                }
            }

            return false;
        }
    }
}