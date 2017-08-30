﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FileHelpers;

namespace Server.Plugins.FieldVisit.StageDischarge.Helpers
{
    public class CsvDateTimeOffsetConverter : ConverterBase
    {
        private readonly string[] _utcOffsets = { "Z", "zzz" };
        private readonly string[] _dateFormats = { "yyyy-MM-ddTHH:mm:ss.0000000", "yyyy-MM-ddTHH:mm:ss" };
        private readonly string[] _supportedDateFormats;

        public CsvDateTimeOffsetConverter()
        {
            _supportedDateFormats = CreateDateTimeFormatsWithUtcOffsets();
        }

        private string[] CreateDateTimeFormatsWithUtcOffsets()
        {
            var formats = new List<string>();
            foreach (string dateFormat in _dateFormats)
            {
                formats.AddRange(_utcOffsets.Select(utcOffset => dateFormat + utcOffset));
            }

            return formats.ToArray();
        }


        public override object StringToField(string from)
        {
            DateTimeOffset dateTimeOffset;
            if (DateTimeOffset.TryParseExact(from, _supportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTimeOffset))
                return dateTimeOffset;

            throw new ConvertException(from, typeof(DateTimeOffset), $"{from} is not in the expected DateTime format: {_supportedDateFormats}");
        }

        public override string FieldToString(object from)
        {
            if (from == null) return "";

            var dateTimeOffset = (DateTimeOffset)from;
            return dateTimeOffset.ToString(_supportedDateFormats[0], CultureInfo.InvariantCulture);
        }
    }
}