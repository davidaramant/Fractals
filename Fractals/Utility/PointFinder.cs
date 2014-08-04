using System;
using System.Threading;
using System.Threading.Tasks;
using Fractals.Arguments;
using Fractals.Model;
using Fractals.PointGenerator;
using log4net;

namespace Fractals.Utility
{
    public class PointFinder
    {
        private static bool _shouldStop = false;
        private readonly static object ShouldStopLock = new object();

        private readonly uint _minimum;
        private readonly uint _maximum;
        private readonly string _outputDirectory;
        private readonly string _outputFile;

        private readonly RandomPointGenerator _pointGenerator;
        
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

        public PointFinder(uint minimum, uint maximum, string outputDirectory, string outputFile, RandomPointGenerator pointGenerator)
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
            _log.DebugFormat("Minimum Threshold: {0}", _minimum);
            _log.DebugFormat("Maximum Threshold: {0}", _maximum);

            var bailout = new BailoutRange(
                minimum: _minimum,
                maximum: _maximum);

            var viewPort = AreaFactory.SearchArea;

            viewPort.LogViewport();

            var list = new ComplexNumberListWriter(_outputDirectory, _outputFile);

            int num = 0;

            Parallel.ForEach(_pointGenerator.GetRandomComplexNumbers(viewPort), new ParallelOptions { MaxDegreeOfParallelism = GlobalArguments.DegreesOfParallelism },
                (number, state) =>
                {
                    if (IsPointInBuddhabrot(number, bailout))
                    {
                        Interlocked.Increment(ref num);
                        list.SaveNumber(number);

                        if (num % 100 == 0)
                        {
                            _log.DebugFormat("Found {0} points", num);
                        }
                    }

                    if (ShouldStop)
                    {
                        state.Break();
                        _log.Debug("This process stopped");
                    }
                });

            _log.DebugFormat("Found {0} points", num);
            _log.Info("Stopped finding points");
        }

        public void Stop()
        {
            ShouldStop = true;
        }


        public static bool IsPointInBuddhabrot(Complex c, BailoutRange bailoutRange)
        {
            double re = 0;
            double im = 0;

            // Check for orbits
            // - Check re/im against an old point
            // - Only check every power of 2
            double oldRe = 0;
            double oldIm = 0;

            uint checkNum = 1;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            double re2 = 0;
            double im2 = 0;

            for (uint i = 0; i < bailoutRange.Maximum; i++)
            {
                var reTemp = re2 - im2 + c.Real;
                im = 2 * re * im + c.Imaginary;
                re = reTemp;

                // Orbit check
                if (checkNum == i)
                {
                    if (IsPracticallyTheSame(oldRe, re) && IsPracticallyTheSame(oldIm, im))
                    {
                        return false;
                    }

                    oldRe = re;
                    oldIm = im;

                    checkNum = checkNum << 1;
                }

                re2 = re * re;
                im2 = im * im;

                // Check the magnitude squared against 2^2
                if ((re2 + im2) > 4)
                {
                    return i >= bailoutRange.Minimum;
                }
            }

            return false;
        }

        private static bool IsPracticallyTheSame(double v1, double v2)
        {
            return Math.Abs(v1 - v2) <= 1e-17;
        }
    }
}