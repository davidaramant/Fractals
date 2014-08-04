namespace Fractals.Model
{
    public sealed class BailoutRange
    {
        public readonly uint Minimum;
        public readonly uint Maximum;

        public BailoutRange(uint maximum)
        {
            Maximum = maximum;
        }

        public BailoutRange(uint minimum, uint maximum)
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
