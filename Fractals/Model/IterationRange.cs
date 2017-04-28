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

        public bool IsInside(int escapeTime)
        {
            return
                escapeTime >= Minimum &&
                escapeTime <= Maximum;
        }

        public override string ToString() => $"{Minimum:N0} <= iterations <= {Maximum:N0}";
    }
}
