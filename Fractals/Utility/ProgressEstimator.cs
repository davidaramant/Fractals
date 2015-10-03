using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fractals.Utility
{
    sealed class ProgressEstimator
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
            var estimatedTotalTicks = (long)(_timer.ElapsedTicks / percentageComplete);
            var estimatedTotalTime = new TimeSpan(estimatedTotalTicks);
            var estimatedEndTime = _startTime + estimatedTotalTime;

            var remaining = estimatedEndTime - DateTime.Now;

            return $"{percentageComplete:P} complete. Estimated end time {estimatedEndTime.TimeOfDay.ToString("hh:mm")} ({remaining.ToString("hh:mm")} remaining)";
        }
    }
}
