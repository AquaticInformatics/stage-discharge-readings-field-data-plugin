﻿using System;
using System.Collections.Generic;
using System.Linq;
using Server.BusinessInterfaces.FieldDataPlugInCore.DataModel.CrossSection;
using Server.BusinessInterfaces.FieldDataPlugInCore.Exceptions;
using Server.Plugins.FieldVisit.CrossSection.Interfaces;
using static System.FormattableString;

namespace Server.Plugins.FieldVisit.CrossSection.Mappers
{
    public class CrossSectionPointMapper : ICrossSectionPointMapper
    {
        public ICollection<CrossSectionPoint> MapPoints(List<Model.CrossSectionPoint> points)
        {
            return points.Where(point => point != null && !point.IsEmptyPoint()).Select(ToPoint).ToList();
        }

        private static CrossSectionPoint ToPoint(Model.CrossSectionPoint point)
        {
            if (!IsValidPoint(point))
                throw new ParsingFailedException(Invariant($"The Cross-Section Point: '{point}' must have both a Distance and Elevation"));

            return new CrossSectionPoint
            {
                Distance = point.Distance.GetValueOrDefault(),
                Elevation = point.Elevation.GetValueOrDefault(),
                Comments = point.Comment
            };
        }

        private static bool IsValidPoint(Model.CrossSectionPoint point)
        {
            return point.Distance.HasValue && point.Elevation.HasValue;
        }
    }
}