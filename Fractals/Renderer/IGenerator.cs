using Fractals.Model;
using System.Drawing;

namespace Fractals.Renderer
{
    public interface IGenerator
    {
        Color[,] Render(Size resolution, Area viewPort);
    }
}
