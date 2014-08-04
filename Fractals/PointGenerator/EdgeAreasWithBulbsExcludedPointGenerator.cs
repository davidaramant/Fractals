using System;
using System.Collections.Generic;
using System.Linq;
using Fractals.Model;
using Fractals.Utility;
using log4net;

namespace Fractals.PointGenerator
{
    public class EdgeAreasWithBulbsExcludedPointGenerator : BulbsExcludedPointGenerator
    {
        private static ILog _log;

        private readonly Random _random;
        private readonly List<Area> _edgeAreas;

        public EdgeAreasWithBulbsExcludedPointGenerator(string directory, string filename)
        {
            _log = LogManager.GetLogger(GetType());

            _random = new Random();
            
            _log.Info("Loading edge areas from file");

            var listReader = new AreaListReader(directory, filename);

            _edgeAreas = listReader
                .GetAreas()
                .ToList();
            _log.DebugFormat("Loaded {0} edge areas", _edgeAreas.Count);
        }

        public override Area SelectArea(Area viewPoint)
        {
            var edgeIndex = _random.Next(_edgeAreas.Count);
            return _edgeAreas[edgeIndex];
        }
    }
}