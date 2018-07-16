using System;
using StageDischargeReadings.Parsers;

namespace StageDischargeReadings.UnitTests.Helpers
{
    internal class StageDischargeRecordBuilder
    {
        private readonly StageDischargeReadingRecord _stageDischargeReadingRecord;

        private StageDischargeRecordBuilder()
        {
            _stageDischargeReadingRecord = new StageDischargeReadingRecord();
        }

        public static StageDischargeRecordBuilder Build()
        {
            return new StageDischargeRecordBuilder();
        }

        public StageDischargeRecordBuilder WithLocationIdentifier(string locationIdentifier)
        {
            _stageDischargeReadingRecord.LocationIdentifier = locationIdentifier;
            return this;
        }

        public StageDischargeRecordBuilder WithMeasurementId(string measurementId)
        {
            _stageDischargeReadingRecord.MeasurementId = measurementId;
            return this;
        }
        public StageDischargeRecordBuilder WithMeasurementStartDateTime(DateTimeOffset measurementStartDateTime)
        {
            _stageDischargeReadingRecord.MeasurementStartDateTime = measurementStartDateTime;
            return this;
        }
        public StageDischargeRecordBuilder WithMeasurementEndDateTime(DateTimeOffset measurementEndDateTime)
        {
            _stageDischargeReadingRecord.MeasurementEndDateTime = measurementEndDateTime;
            return this;
        }
        public StageDischargeRecordBuilder WithStageAtStart(double stageAtStart)
        {
            _stageDischargeReadingRecord.StageAtStart = stageAtStart;
            return this;
        }
        public StageDischargeRecordBuilder WithStageAtEnd(double stageAtEnd)
        {
            _stageDischargeReadingRecord.StageAtEnd = stageAtEnd;
            return this;
        }
        public StageDischargeRecordBuilder WithStageUnits(string stageUnits)
        {
            _stageDischargeReadingRecord.StageUnits = stageUnits;
            return this;
        }
        public StageDischargeRecordBuilder WithDischarge(double discharge)
        {
            _stageDischargeReadingRecord.Discharge = discharge;
            return this;
        }
        public StageDischargeRecordBuilder WithDischargeUnits(string dischargeUnits)
        {
            _stageDischargeReadingRecord.DischargeUnits = dischargeUnits;
            return this;
        }
        public StageDischargeRecordBuilder WithChannelName(string channelName)
        {
            _stageDischargeReadingRecord.ChannelName = channelName;
            return this;
        }
        public StageDischargeRecordBuilder WithChannelWidth(double channelWidth)
        {
            _stageDischargeReadingRecord.ChannelWidth = channelWidth;
            return this;
        }
        public StageDischargeRecordBuilder WithWidthUnits(string widthUnits)
        {
            _stageDischargeReadingRecord.WidthUnits = widthUnits;
            return this;
        }
        public StageDischargeRecordBuilder WithChannelArea(double channelArea)
        {
            _stageDischargeReadingRecord.ChannelArea = channelArea;
            return this;
        }
        public StageDischargeRecordBuilder WithAreaUnits(string areaUnits)
        {
            _stageDischargeReadingRecord.AreaUnits = areaUnits;
            return this;
        }
        public StageDischargeRecordBuilder WithChannelVelocity(double channelVelocity)
        {
            _stageDischargeReadingRecord.ChannelVelocity = channelVelocity;
            return this;
        }
        public StageDischargeRecordBuilder WithVelocityUnits(string velocityUnits)
        {
            _stageDischargeReadingRecord.VelocityUnits = velocityUnits;
            return this;
        }
        public StageDischargeRecordBuilder WithParty(string party)
        {
            _stageDischargeReadingRecord.Party = party;
            return this;
        }
        public StageDischargeRecordBuilder WithComments(string comments)
        {
            _stageDischargeReadingRecord.Comments = comments;
            return this;
        }

        public StageDischargeReadingRecord ARecord()
        {
            // todo: replace with a copy/clone of the internal record to
            // avoid situations where the builder is reused rather than 
            // reinitialized
            return _stageDischargeReadingRecord;
        }
    }
}
