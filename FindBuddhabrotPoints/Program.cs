using System.IO;
using Fractals.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FindBuddhabrotPoints
{
    class Program
    {
        private static bool _shouldStop = false;
        private readonly static object ShouldStopLock = new object();

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

        static void Main(string[] args)
        {
            Console.WriteLine("Press any key to cancel...");

            Task.Factory.StartNew(() =>
            {
                Console.ReadKey();
                ShouldStop = true;
            });

            Parallel.ForEach(GetRandomComplexNumbers(),
                (number, state) =>
                {


                    if (ShouldStop)
                    {
                        state.Break();
                    }
                });
        }

        private static IEnumerable<Complex> GetRandomComplexNumbers()
        {
            var rand = new Random();
            while (true)
            {
                // SO SO WRONG
                yield return new Complex(rand.NextDouble(), rand.NextDouble());
            }
        }
    }
}
