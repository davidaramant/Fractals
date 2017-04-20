namespace Fractals.Model
{
    public sealed class IterationRange
    {
        public readonly int Minimum;
        public readonly int Maximum;

        public IterationRange(int maximum)
        {
            Maximum = maximum;
        }

        public IterationRange(int minimum, int maximum)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public bool IsInside(uint escapeTime)
        {
            return
                escapeTime >= Minimum &&
                escapeTime <= Maximum;
        }
    }
}
