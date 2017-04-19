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

        private static unsafe void DoStuff()
        {
            var source = @"__kernel void add_numbers(
      __global float4* data, 
      __local float* local_result, 
      __global float* group_result) {

   float sum;
   float4 input1, input2, sum_vector;
   uint global_addr, local_addr;

   global_addr = get_global_id(0) * 2;
   input1 = data[global_addr];
   input2 = data[global_addr+1];
   sum_vector = input1 + input2;

   local_addr = get_local_id(0);
   local_result[local_addr] = sum_vector.s0 + sum_vector.s1 + 
                              sum_vector.s2 + sum_vector.s3; 
   barrier(CLK_LOCAL_MEM_FENCE);

   if(get_local_id(0) == 0) {
      sum = 0.0f;
      for(int i=0; i<get_local_size(0); i++) {
         sum += local_result[i];
      }
      group_result[get_group_id(0)] = sum;
   }
}";

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
                        using (var outputBuffer = context.CreateBuffer(MemoryFlags.CopyHostPointer | MemoryFlags.WriteOnly, sizeof(float) * numGroups, (IntPtr)pData))
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
                DoStuff();
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
