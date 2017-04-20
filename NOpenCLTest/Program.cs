using System;
using System.Linq;
using NOpenCL;

namespace NOpenCLTest
{
    class Program
    {
        private static Device GetIntelGpu()
        {
            return Platform.GetPlatforms().SelectMany(p => p.GetDevices()).Single(d => d.DeviceType == DeviceType.Gpu);
        }

        private static unsafe void AddNumbers()
        {
            var source = System.IO.File.ReadAllText("add_numbers.cl");

            using (var device = GetIntelGpu())
            using (var context = Context.Create(device))
            using (var commandQueue = context.CreateCommandQueue(device))
            using (var program = context.CreateProgramWithSource(source))
            {
                program.Build();

                using (var kernel = program.CreateKernel("add_numbers"))
                {
                    int arraySize = 64;
                    var data = Enumerable.Range(0, arraySize).Select(i => (float)i).ToArray();
                    var sum = new float[2];

                    var globalSize = 8;
                    var localSize = 4;

                    var numGroups = globalSize / localSize;

                    fixed (float* pData = data, pSum = sum)
                    {
                        using (var inputBuffer = context.CreateBuffer(MemoryFlags.CopyHostPointer | MemoryFlags.ReadOnly, sizeof(float) * arraySize, (IntPtr)pData))
                        using (var outputBuffer = context.CreateBuffer(MemoryFlags.CopyHostPointer | MemoryFlags.WriteOnly, sizeof(float) * numGroups, (IntPtr)pSum))
                        {
                            kernel.Arguments[0].SetValue(inputBuffer);
                            kernel.Arguments[1].SetValue(size: new UIntPtr((uint)localSize * sizeof(float)), value: IntPtr.Zero);
                            kernel.Arguments[2].SetValue(outputBuffer);

                            using (var perfEvent = commandQueue.EnqueueNDRangeKernel(kernel, globalWorkSize: new[] { (IntPtr)globalSize }, localWorkSize: new[] { (IntPtr)localSize }))
                            {
                                Event.WaitAll(perfEvent);
                            }

                            using (commandQueue.EnqueueReadBuffer(outputBuffer, true, 0, sizeof(float) * sum.Length, (IntPtr)pSum))
                            {
                            }

                            commandQueue.Finish();
                        }
                    }

                    var totalSum = sum.Sum();
                    Console.WriteLine($"GPU Sum: {totalSum}");
                    Console.WriteLine($"Native Sum: {data.Sum()}");
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                AddNumbers();
            }
            catch (Exception e)
            {
                Console.WriteLine("Something blew up..." + System.Environment.NewLine + e);
                Console.WriteLine();
            }

            Console.WriteLine("Press a key to exit...");
            Console.ReadKey();
        }
    }
}
