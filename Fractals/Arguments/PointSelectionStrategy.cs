using System;

namespace Fractals.Arguments
{
    [Serializable]
    public enum PointSelectionStrategy
    {
        Random,
        BulbsExcluded,
        EdgesWithBulbsExcluded,
        BulbsOnly,
        EdgesAndBulbsOnly
    }
}