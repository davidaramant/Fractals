using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Fractals.Model;
using Fractals.Utility;
using log4net;

namespace Fractals.PointGenerator
{
    public class EdgeAreasAndBulbsPointGenerator : BulbsOnlyPointGenerator
    {
        private static ILog _log;

        private readonly List<Area> _edgeAreas;

        public EdgeAreasAndBulbsPointGenerator(string directory, string filename)
        {
            _log = LogManager.GetLogger(GetType());
            
            _log.Info("Loading edge areas from file");

            var listReader = new AreaListReader(directory, filename);

            _edgeAreas = listReader
                .GetAreas()
                .ToList();
            _log.DebugFormat("Loaded {0:N0} edge areas", _edgeAreas.Count);
        }

        protected override bool ValidatePoint(Complex point)
        {
            if (base.ValidatePoint(point))
            {
                return true;
            }

            return _edgeAreas.Any(a => a.IsInside(point));
        }
    }
}