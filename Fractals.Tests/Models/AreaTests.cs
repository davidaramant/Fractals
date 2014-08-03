using System.Drawing;
using Fractals.Model;
using NUnit.Framework;

namespace Fractals.Tests.Models
{
    [TestFixture]
    public class AreaTests
    {
        [Test]
        public void PointToNumberToPointTest()
        {
            const int xRange = 500;
            const int yRange = 500;

            var resolution = new Size(xRange * 2, yRange * 2);

            const int xLower = -1 * xRange;
            const int xUpper = xRange;
            const int yLower = -1 * yRange;
            const int yUpper = yRange;

            var area = new Area(new InclusiveRange(xLower, xUpper), new InclusiveRange(yLower, yUpper));
            for (int x = xLower; x < xUpper; x++)
            {
                for (int y = yLower; y < yUpper; y++)
                {
                    var point = new Point(x, y);

                    var calculatedNumber = area.GetNumberFromPoint(resolution, point);
                    var calculatedPoint = area.GetPointFromNumber(resolution, calculatedNumber);

                    Assert.AreEqual(point, calculatedPoint);
                }
            }
        }

        [Test]
        public void NumberToPointToNumberTest()
        {
            const int xRange = 500;
            const int yRange = 500;

            var resolution = new Size(xRange * 2, yRange * 2);

            const int xLower = -1 * xRange;
            const int xUpper = xRange;
            const int yLower = -1 * yRange;
            const int yUpper = yRange;

            var area = new Area(new InclusiveRange(xLower, xUpper), new InclusiveRange(yLower, yUpper));
            for (int x = xLower; x < xUpper; x++)
            {
                for (int y = yLower; y < yUpper; y++)
                {
                    var number = new Complex(x, y);

                    var calculatedPoint = area.GetPointFromNumber(resolution, number);
                    var calculatedNumber = area.GetNumberFromPoint(resolution, calculatedPoint);

                    Assert.AreEqual(number.Real, calculatedNumber.Real);
                    Assert.AreEqual(number.Imaginary, calculatedNumber.Imaginary);
                }
            }
        }
    }
}