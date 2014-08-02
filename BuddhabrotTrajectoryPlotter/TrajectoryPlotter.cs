using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fractals.Utility;

namespace BuddhabrotTrajectoryPlotter
{
    class TrajectoryPlotter
    {
        static void Main(string[] args)
        {
            const string filename = "output.list";

            var list = new ComplexNumberList(filename);

            Console.Out.WriteLine(list.GetNumbers().Count());
        }
    }
}
