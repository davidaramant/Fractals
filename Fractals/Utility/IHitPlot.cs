using System.Drawing;

namespace Fractals.Utility
{
    public interface IHitPlot
    {
        Size Resolution { get; }

        void SaveTrajectories(string filePath);
        void LoadTrajectories(string filePath);
        void IncrementPoint(Point p);
        int GetHitsForPoint(Point p);
        int Max();
        long Sum();
    }
}