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
using Buffer = NOpenCL.Buffer;

namespace Benchmarks
{
    public class ScalarVsVectorVsGpuPointFinder
    {
        public IterationRange Range => new IterationRange(20_000_000, 30_000_000);

        public static int NumberOfPoints => 8192;
        // 16384 - GTX 1060
        // 12800 - Intel desktop
        // 10240 - Intel laptop

        private static IEnumerable<Area> GetEdges()
        {
            var edgeReader = new AreaListReader(directory: ".", filename: @"NewEdge.edge");
            return edgeReader.GetAreas();
        }

        private readonly RandomPointGenerator _pointGenerator = new RandomPointGenerator(GetEdges(), seed: 0);

        private static readonly string KernelSource = System.IO.File.ReadAllText("iterate_points.cl");

        private static Device GetDevice(DeviceType deviceType)
        {
            return Platform.GetPlatforms()[0].GetDevices(deviceType).ElementAt(0);
        }

        [Setup]
        public void Initialize()
        {
            _pointGenerator.ResetRandom(seed: 0);
        }

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

            for (int i = 0; i < range.ExclusiveMaximum; i++)
            {
                float zRealTemp = zReal2 - zImag2 + cReal;
                zImag = 2 * zReal * zImag + cImag;
                zReal = zRealTemp;

                zReal2 = zReal * zReal;
                zImag2 = zImag * zImag;

                // Check the magnitude squared against 2^2
                if ((zReal2 + zImag2) > 4)
                {
                    return i >= range.InclusiveMinimum;
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

                    var finalIterations = IsBuddhabrotPointVector(cReal, cImag, Range.ExclusiveMaximum);
                    finalIterations.CopyTo(result);

                    buddhabrotPointsFound += result.Count(i => Range.IsInside(i));
                }
            }

            return buddhabrotPointsFound;
        }

        [Benchmark]
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

                    var finalIterations = method(cReal, cImag, Range.ExclusiveMaximum);
                    finalIterations.CopyTo(result);

                    subTotal += result.Count(r => Range.IsInside(r));

                    return subTotal;
                },
                localFinally: count => Interlocked.Add(ref buddhabrotPointsFound, count));

            return buddhabrotPointsFound;
        }

        #endregion Vectors

        #region OpenCL

        [Benchmark]
        public int FindPointsOpenClCpu()
        {
            return FindPointsOpenCl(DeviceType.Cpu);
        }

        [Benchmark]
        public int FindPointsOpenClGpu()
        {
            return FindPointsOpenCl(DeviceType.Gpu);
        }

        [Benchmark]
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
                var program = stack.Add(context.CreateProgramWithSource(KernelSource));
                //program.Build("-cl-no-signed-zeros -cl-finite-math-only"); // TODO: These optimization don't work on nVidia
                program.Build();

                var points =
                    _pointGenerator.GetNumbers().
                        Where(c => !MandelbulbChecker.IsInsideBulbs(c)).
                        Take(NumberOfPoints);

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
                                globalWorkSize: new[] {(IntPtr) batchSizes[deviceIndex]},
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

        private unsafe int FindPointsOpenCl(DeviceType deviceType)
        {
            using (var device = GetDevice(deviceType))
            using (var context = Context.Create(device))
            using (var commandQueue = context.CreateCommandQueue(device))
            using (var program = context.CreateProgramWithSource(KernelSource))
            {
                program.Build();

                using (var kernel = program.CreateKernel("iterate_points"))
                {
                    var points =
                        _pointGenerator.GetNumbers().
                            Where(c => !MandelbulbChecker.IsInsideBulbs(c)).
                            Take(NumberOfPoints);

                    var cReals = new float[NumberOfPoints];
                    var cImags = new float[NumberOfPoints];

                    foreach (var (c, index) in points.Select((c, i) => (c, i)))
                    {
                        cReals[index] = (float)c.Real;
                        cImags[index] = (float)c.Imaginary;
                    }

                    var finalIterations = new int[NumberOfPoints];

                    IntPtr[] globalSize = { (IntPtr)NumberOfPoints };
                    IntPtr[] localSize = null;//{ (IntPtr)device.MaxComputeUnits };

                    fixed (float* pCReals = cReals, pCImags = cImags)
                    fixed (int* pFinalIterations = finalIterations)
                    {
                        using (var cRealsBuffer = context.CreateBuffer(
                            MemoryFlags.CopyHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                            sizeof(float) * NumberOfPoints,
                            (IntPtr)pCReals))
                        using (var cImagsBuffer = context.CreateBuffer(
                            MemoryFlags.CopyHostPointer | MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess,
                            sizeof(float) * NumberOfPoints,
                            (IntPtr)pCImags))
                        using (var iterationsBuffer = context.CreateBuffer(
                            MemoryFlags.UseHostPointer | MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly,
                            sizeof(int) * NumberOfPoints,
                            (IntPtr)pFinalIterations))
                        {
                            kernel.Arguments[0].SetValue(cRealsBuffer);
                            kernel.Arguments[1].SetValue(cImagsBuffer);
                            kernel.Arguments[2].SetValue(iterationsBuffer);

                            using (var perfEvent = commandQueue.EnqueueNDRangeKernel(
                                kernel,
                                globalWorkSize: globalSize,
                                localWorkSize: localSize))
                            {
                                Event.WaitAll(perfEvent);
                            }

                            using (commandQueue.EnqueueReadBuffer(
                                iterationsBuffer,
                                blocking: true,
                                offset: 0,
                                size: sizeof(int) * finalIterations.Length,
                                destination: (IntPtr)pFinalIterations))
                            {
                            }

                            commandQueue.Finish();
                        }
                    }

                    return finalIterations.Count(Range.IsInside);
                }
            }
        }

        #endregion OpenCL
    }
}
