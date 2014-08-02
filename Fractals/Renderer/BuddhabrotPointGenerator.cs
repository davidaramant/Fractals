using Fractals.Model;

namespace Fractals.Renderer
{
    public static class BuddhabrotPointGenerator
    {
        public static bool IsPointInBuddhabrot(Complex c, BailoutRange bailoutRange)
        {
            var rePrev = c.Real;
            var imPrev = c.Imag;

            double re = 0;
            double im = 0;
            
            for (int i = 0; i < bailoutRange.Max; i++)
            {
                var reTemp = re*re - im*im + rePrev;
                im = 2*re*im + imPrev;
                re = reTemp;

                var magnitudeSquared = re*re + im*im;
                if (magnitudeSquared > 4)
                {
                    return i >= bailoutRange.Min;
                }
            }

            return false;
        }
    }
}