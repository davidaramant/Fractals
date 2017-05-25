using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Fractals.Model;
using Fractals.PointGenerator;
using Fractals.Utility;
using NOpenCL;
using Buffer = NOpenCL.Buffer;

namespace Benchmarks
{
    public class IterationBenchmarks
    {
        public IterationRange Range => new IterationRange(10_000_000, 15_000_000);

        private string _contextName;
        private Func<Device, bool> _openCLGuard;
        private bool _shouldRunTest = true;
        public readonly int NumberTrials;
        public readonly int NumberOfPoints;
        // 512 - software
        // 16384 - GTX 1060
        // 12800 - Intel desktop
        // 10240 - Intel laptop

        sealed class TestContext : IDisposable
        {
            private readonly IterationBenchmarks _b;

            public TestContext(IterationBenchmarks b, string name, Func<Device, bool> openCLGuard)
            {
                Console.WriteLine($"\n\n{new string('#', name.Length)}\n{name}\n{new string('#', name.Length)}\n");
                _b = b;
                _b._openCLGuard = openCLGuard;
                _b._contextName = name;
            }

            public void Dispose()
            {
                _b._contextName = null;
            }
        }

        public IDisposable SetContext(string name, Func<Device, bool> openCLGuard = null) =>
            new TestContext(this, name, openCLGuard ?? new Func<Device, bool>(_ => true));


        public IterationBenchmarks(int numberOfPoints , int trials)
        {
            NumberOfPoints = numberOfPoints;
            NumberTrials = trials;
        }

        private static IEnumerable<Area> GetEdges()
        {
            var edgeReader = new AreaListReader(directory: ".", filename: @"NewEdge.edge");
            return edgeReader.GetAreas();
        }

        //        private readonly RandomPointGenerator _pointGenerator = new RandomPointGenerator(GetEdges(), seed: 0);
        private readonly RandomPointGenerator _pointGenerator = new RandomPointGenerator(
            new[] { new Area(
                realRange: new InclusiveRange(-0.0330803080308029, -0.0328162816281627),
                imagRange: new InclusiveRange(0.768261665534427, 0.768525691937067)), }, seed: 0);

        private static readonly string KernelSourceFloat = System.IO.File.ReadAllText("iterate_points_f.cl");
        private static readonly string KernelSourceDouble = System.IO.File.ReadAllText("iterate_points_d.cl");


        public readonly List<Result> Results = new List<Result>();

        public void RunTest(string name, Func<int> test)
        {
            Console.WriteLine($"* {name}:");

            var totals = new int[NumberTrials];

            GC.Collect();

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < NumberTrials; i++)
            {
                InitializeRun();
                totals[i] = test();
            }
            stopwatch.Stop();

            var pointsPerSecond = NumberTrials * NumberOfPoints / stopwatch.Elapsed.TotalSeconds;
            Console.WriteLine($"\t{totals[0]} points (Took {stopwatch.Elapsed.TotalSeconds / NumberTrials:N1}s - {pointsPerSecond:N1} points/sec)");
            Results.Add(new Result(_contextName, name, pointsPerSecond, totals[0]));
        }

        public void RunFloatOpenCLTest(string name, IDisposable setup)
        {
            using (setup)
            {
                if (_shouldRunTest)
                    RunTest(name, OpenCLFloats);
                else
                    Results.Add(new Result(_contextName, name, 0, 0));
            }
        }

        public void RunDoubleOpenCLTest(string name, IDisposable setup)
        {
            using (setup)
            {
                if (_shouldRunTest)
                    RunTest(name, OpenCLDoubles);
                else
                    Results.Add(new Result(_contextName, name, 0, 0));
            }
        }

        private static Device GetDevice(DeviceType deviceType)
        {
            return Platform.GetPlatforms()[0].GetDevices(deviceType).ElementAt(0);
        }

        public void InitializeRun()
        {
            _pointGenerator.ResetRandom(seed: 0);
        }

        private IEnumerable<Complex> GetNumbers() => _pointGenerator.GetNumbers().Take(NumberOfPoints);

        #region Scalar

        public int Scalar() =>
            GetNumbers().
            Count(c => Iterate(c, Range.ExclusiveMaximum) == Range.ExclusiveMaximum);
        public int ScalarParallel() =>
            GetNumbers().
            AsParallel().
            Count(c => Iterate(c, Range.ExclusiveMaximum) == Range.ExclusiveMaximum);

        public int ScalarParallelNoAdt() =>
                GetNumbers()
                .AsParallel()
                .Count(c => IterateNoADT((float)c.Real, (float)c.Imaginary, Range.ExclusiveMaximum) == Range.ExclusiveMaximum);

        public int ScalarParallelNoAdtCachingSquares() =>
                GetNumbers()
                .AsParallel()
                .Count(c => IterateCachingSquares((float)c.Real, (float)c.Imaginary, Range.ExclusiveMaximum) == Range.ExclusiveMaximum);

        public int ScalarParallelNoAdtCachingSquaresDoubles() =>
                GetNumbers()
                .AsParallel()
                .Count(c => IterateDouble(c.Real, c.Imaginary, Range.ExclusiveMaximum) == Range.ExclusiveMaximum);

        public int ScalarParallelNoAdtCachingSquaresCycleDetection() =>
                GetNumbers()
                .AsParallel()
                .Count(c => IterateCycleDetection((float)c.Real, (float)c.Imaginary, Range.ExclusiveMaximum) == Range.ExclusiveMaximum);

        public int Iterate(
            Complex c, int iterationLimit)
        {
            var z = Complex.Zero;

            for (int i = 0; i < iterationLimit; i++)
            {
                z = z * z + c;

                if (z.Magnitude > 2)
                    return i;
            }
            return iterationLimit;
        }

        public int IterateNoADT(
            float cReal, float cImag, int iterationLimit)
        {
            float zReal = 0;
            float zImag = 0;

            for (int i = 0; i < iterationLimit; i++)
            {
                var zRealTemp = zReal * zReal - zImag * zImag + cReal;
                zImag = 2 * zReal * zImag + cImag;
                zReal = zRealTemp;

                if (zReal * zReal + zImag + zImag > 4)
                    return i;
            }
            return iterationLimit;
        }

        public int IterateCachingSquares(
            float cReal, float cImag, int iterationLimit)
        {
            float zReal = 0;
            float zImag = 0;

            float zReal2 = 0;
            float zImag2 = 0;

            for (int i = 0; i < iterationLimit; i++)
            {
                zImag = 2 * zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                if (zReal2 + zImag2 > 4)
                    return i;
            }
            return iterationLimit;
        }

        public int IterateCycleDetection(
            float cReal, float cImag, int iterationLimit)
        {
            float zReal = 0;
            float zImag = 0;

            float zReal2 = 0;
            float zImag2 = 0;

            float oldZReal = 0, oldZImag = 0;

            int stepsTaken = 0;
            int stepLimit = 2;

            for (int i = 0; i < iterationLimit; i++)
            {
                zImag = 2 * zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                if (zReal == oldZReal && zImag == oldZImag)
                    return iterationLimit;

                if (stepsTaken == stepLimit)
                {
                    oldZReal = zReal;
                    oldZImag = zImag;
                    stepsTaken = 0;
                    stepLimit *= 2;
                }

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                if (zReal2 + zImag2 > 4)
                    return i;
            }
            return iterationLimit;
        }

        public int IterateDouble(
            double cReal, double cImag, int iterationLimit)
        {
            double zReal = 0;
            double zImag = 0;

            double zReal2 = 0;
            double zImag2 = 0;

            for (int i = 0; i < iterationLimit; i++)
            {
                zImag = 2 * zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                if (zReal2 + zImag2 > 4)
                    return i;
            }
            return iterationLimit;
        }

        #endregion Scalar

        #region Vectors

        public int Vectors() => FindPointsVectorsParallelBatches(IterateVectorDoubles);

        public Vector<long> IterateVectorDoubles(
            Vector<double> cReal, Vector<double> cImag, int maxIterations)
        {
            var zReal = new Vector<double>(0);
            var zImag = new Vector<double>(0);

            var zReal2 = new Vector<double>(0);
            var zImag2 = new Vector<double>(0);

            var iterations = Vector<long>.Zero;
            var increment = Vector<long>.One;

            do
            {
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                var shouldContinue =
                    Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<double>(4)) &
                    Vector.LessThan(iterations, new Vector<long>(maxIterations));

                increment = increment & shouldContinue;
                iterations += increment;
            } while (increment != Vector<long>.Zero);

            return iterations;
        }

        public int VectorsNoEarlyReturn() => FindPointsVectorsParallelBatches(IterateVectorDoublesNoEarlyReturn);

        public Vector<long> IterateVectorDoublesNoEarlyReturn(
            Vector<double> cReal, Vector<double> cImag, int maxIterations)
        {
            var zReal = new Vector<double>(0);
            var zImag = new Vector<double>(0);

            var zReal2 = new Vector<double>(0);
            var zImag2 = new Vector<double>(0);

            var iterations = Vector<long>.Zero;

            for (int i = 0; i < maxIterations; i++)
            {
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                iterations -= Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<double>(4));
            }

            return iterations;
        }

        public delegate Vector<long> IteratePoints(Vector<double> cReal, Vector<double> cImage, int maxIterations);

        public int FindPointsVectorsParallelBatches(IteratePoints method)
        {
            var points =
                GetNumbers();

            var pointsFound = 0;

            var vectorCapacity = Vector<double>.Count;

            IEnumerable<Complex[]> VectorBatch(IEnumerable<Complex> pointSequence)
            {
                int count = 0;
                var batch = new Complex[vectorCapacity];
                foreach (var complex in pointSequence)
                {
                    batch[count++] = complex;

                    if (count == vectorCapacity)
                    {
                        count = 0;
                        yield return batch;
                        batch = new Complex[vectorCapacity];
                    }
                }
            }

            Parallel.ForEach(
                source: VectorBatch(points),
                localInit: () => 0,
                body: (batch, state, subTotal) =>
                {
                    var realBatch = new double[vectorCapacity];
                    var imagBatch = new double[vectorCapacity];
                    var result = new long[vectorCapacity];

                    for (int i = 0; i < vectorCapacity; i++)
                    {
                        realBatch[i] = batch[i].Real;
                        imagBatch[i] = batch[i].Imaginary;
                    }

                    var cReal = new Vector<double>(realBatch);
                    var cImag = new Vector<double>(imagBatch);

                    var finalIterations = method(cReal, cImag, Range.ExclusiveMaximum);
                    finalIterations.CopyTo(result);

                    subTotal += result.Count(r => r == Range.ExclusiveMaximum);

                    return subTotal;
                },
                localFinally: count => Interlocked.Add(ref pointsFound, count));

            return pointsFound;
        }

        #endregion Vectors

        #region Vectors Floats

        public int VectorsFloats() => FindPointsVectorsParallelBatches(IterateVectorsFloats);
        public int VectorsFloats2() => FindPointsVectorsParallelBatches(IterateVectorsEarlyReturn2);

        public Vector<int> IterateVectorsFloats(
            Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            do
            {
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                var shouldContinue =
                    Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4)) &
                    Vector.LessThan(iterations, new Vector<int>(maxIterations));

                increment = increment & shouldContinue;
                iterations += increment;
            } while (increment != Vector<int>.Zero);

            return iterations;
        }

        public Vector<int> IterateVectorsEarlyReturn2(
            Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            for (int i = 0; i < maxIterations; i++)
            {
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                iterations -= Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));

                var shouldContinue =
                    Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));

                increment = increment & shouldContinue;

                if (increment == Vector<int>.Zero)
                    break;

                iterations += increment;
            }

            return iterations;
        }

        public int VectorsNoEarlyReturnFloats() => FindPointsVectorsParallelBatches(IterateVectorFloatsNoEarlyReturn);

        public Vector<int> IterateVectorFloatsNoEarlyReturn(
            Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;

            for (int i = 0; i < maxIterations; i++)
            {
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                iterations -= Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));
            }

            return iterations;
        }

        public int VectorsNoEarlyReturnFloatsTimes2() => FindPointsVectorsParallelBatches(IterateVectorFloatsNoEarlyReturnTimes2);

        public Vector<int> IterateVectorFloatsNoEarlyReturnTimes2(
            Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;

            for (int i = 0; i < maxIterations; i++)
            {
                zImag = new Vector<float>(2) * zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                iterations -= Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));
            }

            return iterations;
        }

        public int VectorsNoEarlyReturnFloatsConstLimit() => FindPointsVectorsParallelBatches(
            IterateVectorFloatsNoEarlyReturnConstLimit);

        public Vector<int> IterateVectorFloatsNoEarlyReturnConstLimit(
            Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;

            for (int i = 0; i < 15_000_000; i++)
            {
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zReal2 - zImag2 + cReal;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                iterations -= Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));
            }

            return iterations;
        }

        public delegate Vector<int> IteratePointsFloats(Vector<float> cReal, Vector<float> cImage, int maxIterations);

        public int FindPointsVectorsParallelBatches(IteratePointsFloats method)
        {
            var points =
                GetNumbers();

            var pointsFound = 0;

            var vectorCapacity = Vector<float>.Count;

            IEnumerable<Complex[]> VectorBatch(IEnumerable<Complex> pointSequence)
            {
                int count = 0;
                var batch = new Complex[vectorCapacity];
                foreach (var complex in pointSequence)
                {
                    batch[count++] = complex;

                    if (count == vectorCapacity)
                    {
                        count = 0;
                        yield return batch;
                        batch = new Complex[vectorCapacity];
                    }
                }
            }

            Parallel.ForEach(
                source: VectorBatch(points),
                localInit: () => 0,
                body: (batch, state, subTotal) =>
                {
                    var realBatch = new float[vectorCapacity];
                    var imagBatch = new float[vectorCapacity];
                    var result = new int[vectorCapacity];

                    for (int i = 0; i < vectorCapacity; i++)
                    {
                        realBatch[i] = (float)batch[i].Real;
                        imagBatch[i] = (float)batch[i].Imaginary;
                    }

                    var cReal = new Vector<float>(realBatch);
                    var cImag = new Vector<float>(imagBatch);

                    var finalIterations = method(cReal, cImag, Range.ExclusiveMaximum);
                    finalIterations.CopyTo(result);

                    subTotal += result.Count(i => i == Range.ExclusiveMaximum);

                    return subTotal;
                },
                localFinally: count => Interlocked.Add(ref pointsFound, count));

            return pointsFound;
        }

        #endregion Vectors Floats

        #region OpenCL

        private Device _device;
        private Context _context;
        private CommandQueue _commandQueue;
        private NOpenCL.Program _program;
        private Kernel _kernel;
        private bool _maxLimitArg;

        public IDisposable SetupOpenCL(
            DeviceType deviceType,
            bool singlePrecision = true,
            string kernelName = "iterate_points",
            bool relaxedMath = false,
            bool maxLimitArg = false)
        {
            _maxLimitArg = maxLimitArg;

            var stack = new DisposeStack();

            _shouldRunTest = true;
            _device = GetDevice(deviceType);
            stack.Add(_device);

            if (_openCLGuard(_device))
            {
                _shouldRunTest = true;

                _context = Context.Create(_device);
                _commandQueue = _context.CreateCommandQueue(_device);
                _program = _context.CreateProgramWithSource(singlePrecision ? KernelSourceFloat : KernelSourceDouble);
                _program.Build(relaxedMath ? "-cl-fast-relaxed-math" : "");
                _kernel = _program.CreateKernel(kernelName);

                stack.AddParams(
                    _context,
                    _commandQueue,
                    _program,
                    _kernel);
            }
            else
            {
                _shouldRunTest = false;
            }
            return stack;
        }


        public unsafe int OpenCLFloats()
        {
            var points =
                GetNumbers();

            var cReals = new float[NumberOfPoints];
            var cImags = new float[NumberOfPoints];

            foreach (var (c, index) in points.Select((c, i) => (c, i)))
            {
                cReals[index] = (float)c.Real;
                cImags[index] = (float)c.Imaginary;
            }

            var finalIterations = new int[NumberOfPoints];

            IntPtr[] globalSize = { (IntPtr)NumberOfPoints };
            IntPtr[] localSize = null;//{ (IntPtr)_device.MaxComputeUnits };

            fixed (float* pCReals = cReals, pCImags = cImags)
            fixed (int* pFinalIterations = finalIterations)
            {
                using (var cRealsBuffer = _context.CreateBuffer(
                    MemoryFlags.UseHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                    sizeof(float) * NumberOfPoints,
                    (IntPtr)pCReals))
                using (var cImagsBuffer = _context.CreateBuffer(
                    MemoryFlags.UseHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                    sizeof(float) * NumberOfPoints,
                    (IntPtr)pCImags))
                using (var iterationsBuffer = _context.CreateBuffer(
                    MemoryFlags.UseHostPointer | MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly,
                    sizeof(int) * NumberOfPoints,
                    (IntPtr)pFinalIterations))
                {
                    _kernel.Arguments[0].SetValue(cRealsBuffer);
                    _kernel.Arguments[1].SetValue(cImagsBuffer);
                    _kernel.Arguments[2].SetValue(iterationsBuffer);
                    if (_maxLimitArg)
                    {
                        _kernel.Arguments[3].SetValue(Range.ExclusiveMaximum);
                    }

                    using (var perfEvent = _commandQueue.EnqueueNDRangeKernel(
                        _kernel,
                        globalWorkSize: globalSize,
                        localWorkSize: localSize))
                    {
                        Event.WaitAll(perfEvent);
                    }

                    using (_commandQueue.EnqueueReadBuffer(
                        iterationsBuffer,
                        blocking: true,
                        offset: 0,
                        size: sizeof(int) * finalIterations.Length,
                        destination: (IntPtr)pFinalIterations))
                    {
                    }

                    _commandQueue.Finish();
                }
            }

            return finalIterations.Count(c => c == Range.ExclusiveMaximum);
        }

        public unsafe int OpenCLDoubles()
        {
            var points =
                GetNumbers();

            var cReals = new double[NumberOfPoints];
            var cImags = new double[NumberOfPoints];

            foreach (var (c, index) in points.Select((c, i) => (c, i)))
            {
                cReals[index] = c.Real;
                cImags[index] = c.Imaginary;
            }

            var finalIterations = new int[NumberOfPoints];

            IntPtr[] globalSize = { (IntPtr)NumberOfPoints };
            IntPtr[] localSize = null;//{ (IntPtr)_device.MaxComputeUnits };

            fixed (double* pCReals = cReals, pCImags = cImags)
            fixed (int* pFinalIterations = finalIterations)
            {
                using (var cRealsBuffer = _context.CreateBuffer(
                    MemoryFlags.UseHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                    sizeof(double) * NumberOfPoints,
                    (IntPtr)pCReals))
                using (var cImagsBuffer = _context.CreateBuffer(
                    MemoryFlags.UseHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                    sizeof(double) * NumberOfPoints,
                    (IntPtr)pCImags))
                using (var iterationsBuffer = _context.CreateBuffer(
                    MemoryFlags.UseHostPointer | MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly,
                    sizeof(int) * NumberOfPoints,
                    (IntPtr)pFinalIterations))
                {
                    _kernel.Arguments[0].SetValue(cRealsBuffer);
                    _kernel.Arguments[1].SetValue(cImagsBuffer);
                    _kernel.Arguments[2].SetValue(iterationsBuffer);
                    if (_maxLimitArg)
                    {
                        _kernel.Arguments[3].SetValue(Range.ExclusiveMaximum);
                    }

                    using (var perfEvent = _commandQueue.EnqueueNDRangeKernel(
                        _kernel,
                        globalWorkSize: globalSize,
                        localWorkSize: localSize))
                    {
                        Event.WaitAll(perfEvent);
                    }

                    using (_commandQueue.EnqueueReadBuffer(
                        iterationsBuffer,
                        blocking: true,
                        offset: 0,
                        size: sizeof(int) * finalIterations.Length,
                        destination: (IntPtr)pFinalIterations))
                    {
                    }

                    _commandQueue.Finish();
                }
            }

            return finalIterations.Count(c => c == Range.ExclusiveMaximum);
        }


        public unsafe int FindPointsOpenClHeterogenous(
            int cpuRatio = 1,
            int gpuRatio = 1)
        {
            using (var stack = new DisposeStack())
            {
                var platform = Platform.GetPlatforms()[0];

                var cpuDevice = platform.GetDevices(DeviceType.Cpu).Single();
                var gpuDevice = platform.GetDevices(DeviceType.Gpu).Single();

                var devices = new[] { cpuDevice, gpuDevice };

                stack.AddMultiple(devices);

                var context = stack.Add(Context.Create(devices));
                var program = stack.Add(context.CreateProgramWithSource(KernelSourceFloat));
                program.Build();

                var points = GetNumbers();

                var cReals = new float[NumberOfPoints];
                var cImags = new float[NumberOfPoints];

                foreach (var (c, index) in points.Select((c, i) => (c, i)))
                {
                    cReals[index] = (float)c.Real;
                    cImags[index] = (float)c.Imaginary;
                }

                var finalIterations = new int[NumberOfPoints];

                var (cpuBatchSize, gpuBatchSize) =
                    Util.Partition(NumberOfPoints,
                        ratio: (cpuRatio, gpuRatio),
                        batchSize: (128, 128));//((int)cpuDevice.MaxComputeUnits, (int)gpuDevice.MaxComputeUnits));

                Console.WriteLine($"Ratio {cpuRatio}:{gpuRatio} = {cpuBatchSize} CPU, {gpuBatchSize} GPU");

                var batchSizes = new[] { cpuBatchSize, gpuBatchSize };

                fixed (float* pCReals = cReals, pCImags = cImags)
                fixed (int* pFinalIterations = finalIterations)
                {
                    var cRealsBuffer = stack.Add(context.CreateBuffer(
                        MemoryFlags.UseHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                        sizeof(float) * NumberOfPoints,
                        (IntPtr)pCReals));
                    var cImagsBuffer = stack.Add(context.CreateBuffer(
                        MemoryFlags.UseHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                        sizeof(float) * NumberOfPoints,
                        (IntPtr)pCImags));
                    var iterationsBuffer = stack.Add(context.CreateBuffer(
                        MemoryFlags.UseHostPointer | MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly,
                        sizeof(int) * NumberOfPoints,
                        (IntPtr)pFinalIterations));

                    Buffer SplitBuffer(Buffer input, int offset, int size)
                    {
                        return stack.Add(
                            input.CreateSubBuffer(
                                MemoryFlags.None,
                                new BufferRegion((IntPtr)(offset * 4), (IntPtr)(size * 4))));
                    }

                    var cRealsSubBuffers = new[]
                    {
                        SplitBuffer(cRealsBuffer, 0, cpuBatchSize),
                        SplitBuffer(cRealsBuffer, cpuBatchSize, gpuBatchSize)
                    };
                    var cImagsSubBuffers = new[]
                    {
                        SplitBuffer(cImagsBuffer, 0, cpuBatchSize),
                        SplitBuffer(cImagsBuffer, cpuBatchSize, gpuBatchSize)
                    };
                    var iterationsSubBuffers = new[]
                    {
                        SplitBuffer(iterationsBuffer, 0, cpuBatchSize),
                        SplitBuffer(iterationsBuffer, cpuBatchSize, gpuBatchSize)
                    };

                    var commandQueues = new CommandQueue[devices.Length];
                    var enqueueEvents = new Event[devices.Length];
                    for (int deviceIndex = 0; deviceIndex < devices.Length; deviceIndex++)
                    {
                        var device = devices[deviceIndex];
                        commandQueues[deviceIndex] = stack.Add(context.CreateCommandQueue(device));

                        var kernel = stack.Add(program.CreateKernel("iterate_points"));

                        kernel.Arguments[0].SetValue(cRealsSubBuffers[deviceIndex]);
                        kernel.Arguments[1].SetValue(cImagsSubBuffers[deviceIndex]);
                        kernel.Arguments[2].SetValue(iterationsSubBuffers[deviceIndex]);

                        enqueueEvents[deviceIndex] = commandQueues[deviceIndex]
                            .EnqueueNDRangeKernel(
                                kernel,
                                globalWorkSize: new[] { (IntPtr)batchSizes[deviceIndex] },
                                localWorkSize: null);//new[] {(IntPtr) device.MaxComputeUnits});
                    }

                    Event.WaitAll(enqueueEvents);

                    for (int deviceIndex = 0; deviceIndex < devices.Length; deviceIndex++)
                    {
                        using (commandQueues[deviceIndex].EnqueueReadBuffer(
                                iterationsSubBuffers[deviceIndex],
                                blocking: true,
                                offset: 0,
                                size: sizeof(int) * batchSizes[deviceIndex],
                                destination: (IntPtr)(pFinalIterations + deviceIndex * cpuBatchSize)))
                        {
                        }

                        commandQueues[deviceIndex].Finish();
                    }

                    stack.AddMultiple(commandQueues);

                    return finalIterations.Count(Range.IsInside);
                }
            }
        }

        #endregion OpenCL
    }
}
