using System;
using OpenCL.Net;
using OpenCL.Net.Extensions;

namespace OpenCLNetTest
{
    class Program
    {
        private static void DoStuff()
        {
            using (var env = "*Intel*".CreateCLEnvironment(deviceType: DeviceType.All))
            {

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
