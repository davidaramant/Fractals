namespace Fractals.Model
{
    public sealed class BailoutRange
    {
        public readonly int Minimum;
        public readonly int Maximum;

        public BailoutRange(int maximum)
        {
            Maximum = maximum;
        }

        public BailoutRange(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public bool IsInside(int escapeTime)
        {
            return
                escapeTime >= Minimum &&
                escapeTime <= Maximum;
        }
    }
}
