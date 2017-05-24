namespace Benchmarks
{
    public sealed class Result
    {
        public readonly string Context;
        public readonly string Name;
        public readonly double Rate;
        public readonly int PointsFound;

        public Result(string context, string name, double rate, int pointsFound)
        {
            Context = context;
            Name = name;
            Rate = rate;
            PointsFound = pointsFound;
        }
    }    
}
