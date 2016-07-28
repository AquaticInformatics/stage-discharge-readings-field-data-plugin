﻿using System;
using FileHelpers;
using Server.Plugins.FieldVisit.CrossSection.Helpers;

namespace Server.Plugins.FieldVisit.CrossSection.Model
{
    [DelimitedRecord(CrossSectionParserConstants.DataRecordSeparator), IgnoreFirst(1)]
    public class CrossSectionPoint
    {
        [FieldOrder(1), FieldTrim(TrimMode.Both), FieldOptional]
        public double? Distance;

        [FieldOrder(2), FieldTrim(TrimMode.Both), FieldOptional]
        public double? Elevation;

        [FieldOrder(3), FieldTrim(TrimMode.Both), FieldQuoted, FieldOptional]
        public string Comment;

        public bool IsEmptyPoint()
        {
            return !Distance.HasValue && !Elevation.HasValue && string.IsNullOrWhiteSpace(Comment);
        }

        public override string ToString()
        {
            return FormattableString.Invariant($"Distance='{Distance}' Elevation='{Elevation}' Comment='{Comment}'");
        }
    }
}