using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Fractals.Arguments;
using Fractals.Model;
using Fractals.PointGenerator;
using log4net;

namespace Fractals.Utility
{
    public abstract class PointFinder
    {
        private static volatile bool _shouldStop = false;
        private static readonly object ShouldStopLock = new object();

        private readonly int _minimum;
        private readonly int _maximum;
        private readonly string _outputDirectory;
        private readonly string _outputFile;

        private readonly IRandomPointGenerator _pointGenerator;

        private static ILog _log;

        private static bool ShouldStop
        {
            get
            {
                lock (ShouldStopLock)
                {
                    return _shouldStop;
                }
            }
            set
            {
                lock (ShouldStopLock)
                {
                    _shouldStop = value;
                }
            }
        }

        protected PointFinder(int minimum, int maximum, string outputDirectory, string outputFile, IRandomPointGenerator pointGenerator)
        {
            _minimum = minimum;
            _maximum = maximum;
            _outputDirectory = outputDirectory;
            _outputFile = outputFile;

            _pointGenerator = pointGenerator;

            _log = LogManager.GetLogger(GetType());
        }

        public void Start()
        {
            _log.Info("Starting to find points");
            _log.DebugFormat("Random Generator: {0}", _pointGenerator.GetType().Name);
            _log.DebugFormat("Minimum Threshold: {0:N0}", _minimum);
            _log.DebugFormat("Maximum Threshold: {0:N0}", _maximum);

            var iterationRange = new IterationRange(
                minimum: _minimum,
                maximum: _maximum);

            var list = new ComplexNumberListWriter(_outputDirectory, _outputFile);

            int num = 0;

            Parallel.ForEach(
                _pointGenerator.GetNumbers().Where(c => !MandelbulbChecker.IsInsideBulbs(c)),
                new ParallelOptions { MaxDegreeOfParallelism = GlobalArguments.DegreesOfParallelism },
                (number, state) =>
                {
                    if (ValidatePoint(number, iterationRange))
                    {
                        Interlocked.Increment(ref num);
                        list.SaveNumber(number);

                        if (num % 100 == 0)
                        {
                            _log.DebugFormat("Found {0:N0} points", num);
                        }
                    }

                    if (ShouldStop)
                    {
                        state.Break();
                        _log.Debug("This process stopped");
                    }
                });

            _log.DebugFormat("Found {0:N0} points", num);
            _log.Info("Stopped finding points");
        }

        public void Stop()
        {
            ShouldStop = true;
        }

        protected abstract bool ValidatePoint(Complex c, IterationRange iterationRange);
    }
}