﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Fractals.Model;
using Fractals.PointGenerator;
using Fractals.Utility;
using NOpenCL;
using System.Collections.Concurrent;

namespace Benchmarks
{
    public class ScalarVsVectorVsGpuPointFinder
    {
        public IterationRange Range { get; set; } = new IterationRange(1_000_000, 5_000_000);


        private int NumberOfBatches => 10;
        private int BatchSize => Vector<float>.Count;
        private int NumberOfPoints => BatchSize * NumberOfBatches;

        private static IEnumerable<Area> GetEdges()
        {
            var edgeReader = new AreaListReader(directory: @"C:\Users\aramant\Desktop\Buddhabrot\Test Plot", filename: @"NewEdge.edge");
            return edgeReader.GetAreas();
        }

        private readonly RandomPointGenerator _pointGenerator = new RandomPointGenerator(GetEdges(), seed: 0);


        private DisposeStack _disposeStack;
        private static readonly string KernelSource = System.IO.File.ReadAllText("iterate_points.cl");

        private static Device GetDevice()
        {
            return Platform.GetPlatforms()[0].GetDevices()[0];
            //return Platform.GetPlatforms().SelectMany(p => p.GetDevices()).Single(d => d.DeviceType == DeviceType.Gpu);
        }

        private Device _device;
        private Context _context;
        private CommandQueue _commandQueue;
        private Kernel _kernel;

        [Setup]
        public void Initialize()
        {
            _pointGenerator.ResetRandom(seed: 0);

            InitializeGpu();
        }

        public void InitializeGpu()
        {
            _disposeStack = new DisposeStack();

            _device = _disposeStack.Add(GetDevice());
            _context = _disposeStack.Add(Context.Create(_device));
            _commandQueue = _disposeStack.Add(_context.CreateCommandQueue(_device));
            var program = _disposeStack.Add(_context.CreateProgramWithSource(KernelSource));

            program.Build("-cl-no-signed-zeros -cl-finite-math-only");

            _kernel = _disposeStack.Add(program.CreateKernel("iterate_points"));
        }

        [Cleanup]
        public void Cleanup() => _disposeStack.Dispose();

        #region Scalar

        //[Benchmark(Baseline = true)]
        public int FindPointsScalar()
        {
            return
                _pointGenerator.GetNumbers()
                .Where(c => !MandelbulbChecker.IsInsideBulbs(c))
                .Take(NumberOfPoints)
                .Count(c => IsBuddhabrotPointScalar((float)c.Real, (float)c.Imaginary, Range));
        }

        //[Benchmark]
        public int FindPointsScalarParallel()
        {
            return
                _pointGenerator.GetNumbers()
                    .Where(c => !MandelbulbChecker.IsInsideBulbs(c))
                    .Take(NumberOfPoints)
                    .AsParallel()
                    .Count(c => IsBuddhabrotPointScalar((float)c.Real, (float)c.Imaginary, Range));
        }

        public bool IsBuddhabrotPointScalar(float cReal, float cImag, IterationRange range)
        {
            float zReal = 0;
            float zImag = 0;

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            float zReal2 = 0;
            float zImag2 = 0;

            for (int i = 0; i < range.Maximum; i++)
            {
                float zRealTemp = zReal2 - zImag2 + cReal;
                zImag = 2 * zReal * zImag + cImag;
                zReal = zRealTemp;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                // Check the magnitude squared against 2^2
                if ((zReal2 + zImag2) > 4)
                {
                    return i >= range.Minimum;
                }
            }

            return true;
        }

        #endregion Scalar

        #region Vectors

        //[Benchmark]
        public int FindPointsVectors()
        {
            var realBatch = new float[Vector<float>.Count];
            var imagBatch = new float[Vector<float>.Count];
            var result = new int[Vector<int>.Count];

            var points =
                _pointGenerator.GetNumbers().
                    Where(c => !MandelbulbChecker.IsInsideBulbs(c)).
                    Take(NumberOfPoints);

            var buddhabrotPointsFound = 0;

            int batchProgress = 0;
            foreach (var c in points)
            {
                realBatch[batchProgress] = (float)c.Real;
                imagBatch[batchProgress] = (float)c.Imaginary;

                batchProgress++;

                if (batchProgress == Vector<float>.Count)
                {
                    batchProgress = 0;

                    var cReal = new Vector<float>(realBatch);
                    var cImag = new Vector<float>(imagBatch);

                    var finalIterations = IsBuddhabrotPointVector(cReal, cImag, Range.Maximum);
                    finalIterations.CopyTo(result);

                    buddhabrotPointsFound += result.Count(i => Range.IsInside(i));
                }
            }

            return buddhabrotPointsFound;
        }

        //[Benchmark]
        public int FindPointsVectorsParallel()
        {
            return FindPointsVectorsParallelBatches(IsBuddhabrotPointVector);
        }

        public Vector<int> IsBuddhabrotPointVector(Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            do
            {
                var zRealTemp = zReal2 - zImag2 + cReal;
                // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
                // I don't get it either
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zRealTemp;

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

        [Benchmark]
        public int FindPointsVectorsNoEarlyReturn()
        {
            return FindPointsVectorsParallelBatches(IsBuddhabrotPointVectorNoEarlyReturn);
        }

        public Vector<int> IsBuddhabrotPointVectorNoEarlyReturn(Vector<float> cReal, Vector<float> cImag, int maxIterations)
        {
            var zReal = new Vector<float>(0);
            var zImag = new Vector<float>(0);

            // Cache the squares
            // They are used to find the magnitude; reuse these values when computing the next re/im
            var zReal2 = new Vector<float>(0);
            var zImag2 = new Vector<float>(0);

            var iterations = Vector<int>.Zero;
            var increment = Vector<int>.One;

            for (int i = 0; i < maxIterations; i++)
            {
                var zRealTemp = zReal2 - zImag2 + cReal;
                // Doing the two multiplications with an addition is somehow faster than 2 * zReal * zImag!
                // I don't get it either
                zImag = zReal * zImag + zReal * zImag + cImag;
                zReal = zRealTemp;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                var shouldContinue = Vector.LessThanOrEqual(zReal2 + zImag2, new Vector<float>(4));

                increment = increment & shouldContinue;
                iterations += increment;
            }

            return iterations;
        }

        public delegate Vector<int> IteratePoints(Vector<float> cReal, Vector<float> cImage, int maxIterations);

        public int FindPointsVectorsParallelBatches(IteratePoints method)
        {
            var points =
                _pointGenerator.GetNumbers().
                    Where(c => !MandelbulbChecker.IsInsideBulbs(c)).
                    Take(NumberOfPoints);

            var buddhabrotPointsFound = 0;

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

                    var finalIterations = method(cReal, cImag, Range.Maximum);
                    finalIterations.CopyTo(result);

                    subTotal += result.Count(r => Range.IsInside(r));

                    return subTotal;
                },
                localFinally: count => Interlocked.Add(ref buddhabrotPointsFound, count));

            return buddhabrotPointsFound;
        }

        #endregion Vectors

        #region OpenCL

        //[Benchmark]
        public unsafe int FindPointsOpenCL()
        {
            var points =
                _pointGenerator.GetNumbers().
                    Where(c => !MandelbulbChecker.IsInsideBulbs(c)).
                    Take(NumberOfPoints).
                    ToArray();

            var cReals = points.Select(c => (float)c.Real).ToArray();
            var cImags = points.Select(c => (float)c.Imaginary).ToArray();
            var finalIterations = new int[NumberOfPoints];

            var globalSize = NumberOfPoints;
            var localSize = 1;

            fixed (float* pCReals = cReals, pCImags = cImags)
            fixed (int* pFinalIterations = finalIterations)
            {
                using (var cRealsBuffer = _context.CreateBuffer(
                    MemoryFlags.CopyHostPointer | MemoryFlags.ReadOnly,
                    sizeof(float) * NumberOfPoints,
                    (IntPtr)pCReals))
                using (var cImagsBuffer = _context.CreateBuffer(
                    MemoryFlags.CopyHostPointer | MemoryFlags.ReadOnly,
                    sizeof(float) * NumberOfPoints,
                    (IntPtr)pCImags))
                using (var iterationsBuffer = _context.CreateBuffer(
                    MemoryFlags.CopyHostPointer | MemoryFlags.WriteOnly,
                    sizeof(int) * NumberOfPoints,
                    (IntPtr)pFinalIterations))
                {
                    _kernel.Arguments[0].SetValue(cRealsBuffer);
                    _kernel.Arguments[1].SetValue(cImagsBuffer);
                    _kernel.Arguments[2].SetValue(Range.Maximum);
                    _kernel.Arguments[3].SetValue(iterationsBuffer);

                    using (var perfEvent = _commandQueue.EnqueueNDRangeKernel(_kernel,
                        globalWorkSize: (IntPtr)globalSize, localWorkSize: (IntPtr)localSize))
                    {
                        Event.WaitAll(perfEvent);
                    }

                    using (_commandQueue.EnqueueReadBuffer(iterationsBuffer, blocking: true, offset: 0,
                        size: sizeof(int) * finalIterations.Length, destination: (IntPtr)pFinalIterations))
                    {
                    }

                    _commandQueue.Finish();
                }
            }

            return finalIterations.Count(Range.IsInside);
        }

        #endregion OpenCL
    }
}