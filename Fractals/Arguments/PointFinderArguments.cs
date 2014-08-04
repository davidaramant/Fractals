using System;

namespace Fractals.Arguments
{
    [Serializable]
    public class PointFinderArguments
    {
        public string OutputDirectory { get; set; }

        public string OutputFilenamePrefix { get; set; }

        public uint MinimumThreshold { get; set; }

        public uint MaximumThreshold { get; set; }

        public PointSelectionStrategy SelectionStrategy { get; set; }
    }
}
