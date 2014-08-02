using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fractals.Model;
using Fractals.Renderer;
using log4net;

namespace Fractals.Utility
{
    public class PointFinder
    {
        private static bool _shouldStop = false;
        private readonly static object ShouldStopLock = new object();

        private readonly int _minimum;
        private readonly int _maximum;
        private readonly string _outputDirectory;
        private readonly string _outputFile;
        
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

        public PointFinder(string outputDirectory, string outputFile)
            : this(2000, 3000, outputDirectory, outputFile)
        {
        }


        public PointFinder(int minimum, int maximum, string outputDirectory, string outputFile)
        {
            _minimum = minimum;
            _maximum = maximum;
            _outputDirectory = outputDirectory;
            _outputFile = outputFile;
            
            _log = LogManager.GetLogger(GetType());
        }

        public void Start()
        {
            _log.InfoFormat("Starting to find points");

            var bailout = new BailoutRange(
                min: _minimum,
                max: _maximum);

            var viewPort = new Area(
                realRange: new InclusiveRange(-2, 0.5),
                imagRange: new InclusiveRange(-1.3, 1.3));

            var list = new ComplexNumberList(_outputDirectory, _outputFile);

            int num = 0;

            Parallel.ForEach(GetRandomComplexNumbers(viewPort),
                (number, state) =>
                {
                    if (BuddhabrotPointGenerator.IsPointInBuddhabrot(number, bailout))
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
                        _log.DebugFormat("This process stopped");
                    }
                });

            _log.InfoFormat("Stopped finding points");
        }

        public void Stop()
        {
            ShouldStop = true;
        }

        private static IEnumerable<Complex> GetRandomComplexNumbers(Area viewPort)
        {
            var rand = new CryptoRandom();
            while (true)
            {
                yield return BuddhabrotPointGenerator.GetPossiblePoint(rand, viewPort);
            }
        } 
    }
}