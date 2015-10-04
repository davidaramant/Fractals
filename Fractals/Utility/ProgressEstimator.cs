using System;
using System.Diagnostics;

namespace Fractals.Utility
{
    public sealed class ProgressEstimator
    {
        readonly DateTime _startTime = DateTime.Now;
        readonly Stopwatch _timer = Stopwatch.StartNew();

        private ProgressEstimator()
        {          
        }

        public static ProgressEstimator Start()
        {
            return new ProgressEstimator();
        }

        public string GetEstimate(double percentageComplete)
        {
            var elapsedTicks = _timer.ElapsedTicks;
            var estimatedTotalTicks = (long)(elapsedTicks / percentageComplete);
            var estimatedTotalTime = new TimeSpan(estimatedTotalTicks);
            try
            {
                var estimatedEndTime = _startTime + estimatedTotalTime;

                var remaining = estimatedEndTime - DateTime.Now;

                return $"{percentageComplete:P} complete. Estimated end time {estimatedEndTime.TimeOfDay.ToString(@"hh\:mm")} ({remaining.ToString(@"hh\:mm")} remaining)";
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.Out.WriteLine($"% complete {percentageComplete}\telapsed ticks: {elapsedTicks}");
                throw;
            }
        }
    }
}
