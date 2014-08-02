namespace Fractals.Model
{
    public sealed class BailoutRange
    {
        public readonly int Min;
        public readonly int Max;

        public BailoutRange(int max)
        {
            Max = max;
        }

        public BailoutRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public bool IsInside(int escapeTime)
        {
            return
                escapeTime >= Min &&
                escapeTime <= Max;
        }
    }
}
