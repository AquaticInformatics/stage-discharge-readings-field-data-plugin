using System;
using System.Collections.Generic;
using System.Linq;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.Readings;
using FieldDataPluginFramework.Validation;
using FileHelpers;
using StageDischargeReadings.Helpers;
using StageDischargeReadings.Interfaces;

namespace StageDischargeReadings.Parsers
{
    [DelimitedRecord(CsvParserConstants.FieldDelimiter)]
    public class StageDischargeReadingRecord : ISelfValidator
    {
        [FieldOrder(1), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow)]
        public string LocationIdentifier;

        [FieldOrder(2), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow)]
        public string MeasurementId;

        [FieldOrder(3), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldConverter(typeof(CsvDateTimeOffsetConverter))]
        public DateTimeOffset MeasurementStartDateTime;

        [FieldOrder(4), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldConverter(typeof(CsvDateTimeOffsetConverter))]
        public DateTimeOffset MeasurementEndDateTime;

        [FieldOrder(5), FieldTrim(TrimMode.Both)]
        public double? StageAtStart;

        [FieldOrder(6), FieldTrim(TrimMode.Both)]
        public double? StageAtEnd;

        [FieldOrder(7), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow)]
        public string StageUnits;

        [FieldOrder(8), FieldTrim(TrimMode.Both)]
        public double? Discharge;

        [FieldOrder(9), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow)]
        public string DischargeUnits;

        [FieldOrder(10), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ChannelName;

        [FieldOrder(11), FieldTrim(TrimMode.Both), FieldOptional]
        public double? ChannelWidth;
        
        [FieldOrder(12), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string WidthUnits;

        [FieldOrder(13), FieldTrim(TrimMode.Both), FieldOptional]
        public double? ChannelArea;

        [FieldOrder(14), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string AreaUnits;

        [FieldOrder(15), FieldTrim(TrimMode.Both), FieldOptional]
        public double? ChannelVelocity;

        [FieldOrder(16), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string VelocityUnits;

        [FieldOrder(17), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string Party;

        [FieldOrder(18), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string Comments;

        [FieldOrder(19), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingParameter;
        [FieldOrder(20), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingUnits;
        [FieldOrder(21), FieldTrim(TrimMode.Both), FieldOptional]
        public double? ReadingValue;

        [FieldOrder(22), FieldTrim(TrimMode.Both), FieldOptional]
        public ReadingType? ReadingType;
        [FieldOrder(23), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingMethod;
        [FieldOrder(24), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingPublish;
        [FieldOrder(25), FieldTrim(TrimMode.Both), FieldOptional]
        public double? ReadingUncertainty;
        [FieldOrder(26), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingDeviceManufacturer;
        [FieldOrder(27), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingDeviceModel;
        [FieldOrder(28), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingDeviceSerialNumber;
        [FieldOrder(29), FieldTrim(TrimMode.Both), FieldQuoted(QuoteMode.OptionalForBoth, MultilineMode.NotAllow), FieldOptional]
        public string ReadingSublocation;

        [FieldHidden]
        public List<Reading> Readings = new List<Reading>();

        public void Validate()
        {
            ValidationChecks.CannotBeNullOrEmpty(nameof(LocationIdentifier), LocationIdentifier);

            if (Discharge.HasValue)
            {
                ValidateDischargeValues();
            }
            else
            {
                ValidateEmptyDischarge();
            }

            ValidateReading();

            ThrowIfNoDischargeOrReadings();

            ValidationChecks
                .MustBeAValidInterval(nameof(MeasurementStartDateTime), MeasurementStartDateTime, 
                                      nameof(MeasurementEndDateTime), MeasurementEndDateTime);
        }

        private void ValidateReading()
        {
            if (!string.IsNullOrEmpty(ReadingParameter) && !string.IsNullOrEmpty(ReadingUnits) && ReadingValue.HasValue)
            {
                var readingTime = GetHumanReadableMidpoint(new DateTimeInterval(MeasurementStartDateTime, MeasurementEndDateTime));

                var readingPublish = ParseNullableBoolean(ReadingPublish);

                var reading = new Reading(ReadingParameter, new Measurement(ReadingValue.Value, ReadingUnits))
                {
                    Comments = Comments,
                    DateTimeOffset = readingTime,
                    ReadingType = ReadingType ?? FieldDataPluginFramework.DataModel.Readings.ReadingType.Unknown,
                    Uncertainty = ReadingUncertainty,
                    SubLocation = string.IsNullOrEmpty(ReadingSublocation) ? null : ReadingSublocation
                };

                if (!string.IsNullOrEmpty(ReadingMethod))
                    reading.Method = ReadingMethod;

                if (readingPublish.HasValue)
                    reading.Publish = readingPublish.Value;

                if (!string.IsNullOrEmpty(ReadingDeviceManufacturer) && !string.IsNullOrEmpty(ReadingDeviceModel) &&
                    !string.IsNullOrEmpty(ReadingDeviceSerialNumber))
                {
                    reading.MeasurementDevice = new MeasurementDevice(ReadingDeviceManufacturer, ReadingDeviceModel, ReadingDeviceSerialNumber);
                }
                else
                {
                    ThrowIfNotNullOrEmpty(nameof(ReadingDeviceManufacturer), ReadingDeviceManufacturer, "Device");
                    ThrowIfNotNullOrEmpty(nameof(ReadingDeviceModel), ReadingDeviceModel, "Device");
                    ThrowIfNotNullOrEmpty(nameof(ReadingDeviceSerialNumber), ReadingDeviceSerialNumber, "Device");
                }

                Readings.Add(reading);

                return;
            }

            var readingKey = nameof(ReadingValue);

            ThrowIfNotNull(nameof(ReadingValue), ReadingValue, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingParameter), ReadingParameter, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingUnits), ReadingUnits, readingKey);
            ThrowIfNotNull(nameof(ReadingType), ReadingType, readingKey);
            ThrowIfNotNull(nameof(ReadingUncertainty), ReadingUncertainty, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingMethod), ReadingMethod, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingDeviceManufacturer), ReadingDeviceManufacturer, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingDeviceModel), ReadingDeviceModel, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingDeviceSerialNumber), ReadingDeviceSerialNumber, readingKey);
            ThrowIfNotNullOrEmpty(nameof(ReadingSublocation), ReadingSublocation, readingKey);
        }

        private static DateTimeOffset GetHumanReadableMidpoint(DateTimeInterval interval)
        {
            var duration = interval.End - interval.Start;
            var midpoint = interval.Start + TimeSpan.FromTicks(duration.Ticks / 2);

            var truncatedTime = new DateTimeOffset(
                midpoint.Year,
                midpoint.Month,
                midpoint.Day,
                midpoint.Hour,
                midpoint.Minute,
                0,
                midpoint.Offset);

            return truncatedTime < interval.Start ? interval.Start : truncatedTime;
        }

        private bool? ParseNullableBoolean(string from)
        {
            if (bool.TryParse(from, out var value))
                return value;

            if (TrueValues.Contains(from))
                return true;

            if (FalseValues.Contains(from))
                return false;

            return null;
        }

        private static readonly HashSet<string> TrueValues =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "y",
                "yes",
                "1",
                "true"
            };

        private static readonly HashSet<string> FalseValues =
            new HashSet<string>(StringComparer.InvariantCultureIgnoreCase)
            {
                "n",
                "no",
                "0",
                "false"
            };


        private void ValidateDischargeValues()
        {
            if (string.IsNullOrEmpty(StageUnits))
            {
                ThrowIfNotNull(nameof(StageAtStart), StageAtStart);
                ThrowIfNotNull(nameof(StageAtEnd), StageAtEnd);
            }

            ValidationChecks.CannotBeNull(nameof(Discharge), Discharge);
            ValidationChecks.CannotBeNullOrEmpty(nameof(DischargeUnits), DischargeUnits);
            ValidationChecks.CannotBeNullOrEmpty(nameof(ChannelName), ChannelName);
            ValidationChecks.CannotBeNullOrEmpty(nameof(WidthUnits), WidthUnits);
            ValidationChecks.CannotBeNullOrEmpty(nameof(AreaUnits), AreaUnits);
            ValidationChecks.CannotBeNullOrEmpty(nameof(VelocityUnits), VelocityUnits);
        }

        private void ValidateEmptyDischarge()
        {
            ThrowIfNotNull(nameof(StageAtStart), StageAtStart);
            ThrowIfNotNull(nameof(StageAtEnd), StageAtEnd);
            ThrowIfNotNullOrEmpty(nameof(StageUnits), StageUnits);
            ThrowIfNotNull(nameof(Discharge), Discharge);
            ThrowIfNotNullOrEmpty(nameof(DischargeUnits), DischargeUnits);
            ThrowIfNotNullOrEmpty(nameof(ChannelName), ChannelName);
            ThrowIfNotNullOrEmpty(nameof(WidthUnits), WidthUnits);
            ThrowIfNotNullOrEmpty(nameof(AreaUnits), AreaUnits);
            ThrowIfNotNullOrEmpty(nameof(VelocityUnits), VelocityUnits);
        }

        private void ThrowIfNotNull<T>(string propertyName, T? value, string keyName = nameof(Discharge)) where T : struct
        {
            if (!value.HasValue) return;

            throw new ArgumentException($"{propertyName} must be empty when {keyName} is not set.");
        }

        private void ThrowIfNotNullOrEmpty(string propertyName, string value, string keyName = nameof(Discharge))
        {
            if (string.IsNullOrEmpty(value)) return;

            throw new ArgumentException($"{propertyName} must be empty when {keyName} is not set.");
        }

        private void ThrowIfNoDischargeOrReadings()
        {
            if (Discharge.HasValue || Readings.Any() || !string.IsNullOrWhiteSpace(Comments)) return;

            throw new ArgumentException($"Each row must contain at least one stage/discharge pair or at least one reading.");
        }
    }
}
