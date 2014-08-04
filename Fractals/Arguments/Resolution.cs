using System;
using System.Drawing;

namespace Fractals.Arguments
{
    [Serializable]
    public class Resolution
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public Size ToSize()
        {
            return new Size(Width, Height);
        }
    }
}