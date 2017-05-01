namespace Fractals.Model
{
    public sealed class IterationRange
    {
        public readonly int InclusiveMinimum;
        public readonly int ExclusiveMaximum;

        public IterationRange(int exclusiveMaximum)
        {
            ExclusiveMaximum = exclusiveMaximum;
        }

        public IterationRange(int inclusiveMinimum, int exclusiveMaximum)
        {
            InclusiveMinimum = inclusiveMinimum;
            ExclusiveMaximum = exclusiveMaximum;
        }

        public bool IsInside(int escapeTime)
        {
            return
                escapeTime >= InclusiveMinimum &&
                escapeTime < ExclusiveMaximum;
        }

        public override string ToString() => $"{InclusiveMinimum:N0} <= iterations < {ExclusiveMaximum:N0}";
    }
}
