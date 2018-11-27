using System;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using StageDischargeReadings.Parsers;
using StageDischargeReadings.UnitTests.TestData;

namespace StageDischargeReadings.UnitTests
{
    [TestFixture]
    public class StageDischargeReadingRecordTests
    {
        private IFixture _fixture;

        [SetUp]
        public void BeforeTests()
        {
            _fixture = new Fixture();
        }

        [TestCase(nameof(StageDischargeReadingRecord.LocationIdentifier))]
        [TestCase(nameof(StageDischargeReadingRecord.DischargeUnits))]
        [TestCase(nameof(StageDischargeReadingRecord.ChannelName))]
        [TestCase(nameof(StageDischargeReadingRecord.WidthUnits))]
        [TestCase(nameof(StageDischargeReadingRecord.AreaUnits))]
        [TestCase(nameof(StageDischargeReadingRecord.VelocityUnits))]
        public void StageDischargeRecord_SelfValidateWithNullPropertyValue_DetectsNull(string propertyName)
        {
            var stageDischargeRecord = StageDischargeCsvFileBuilder.CreateFullRecord(_fixture);
            CheckExpectedExceptionAndMessageWhenSpecifiedFieldIsNull<ArgumentNullException>(stageDischargeRecord, propertyName);
        }

        private void CheckExpectedExceptionAndMessageWhenSpecifiedFieldIsNull<E>(StageDischargeReadingRecord stageDischargeReadingRecord, string propertyName) where E : Exception
        {
            SetValueToNull(ref stageDischargeReadingRecord, propertyName);

            CheckExpectedExceptionForProperty<E>(stageDischargeReadingRecord, propertyName);
        }

        private void CheckExpectedExceptionForProperty<E>(StageDischargeReadingRecord stageDischargeReadingRecord, string propertyName) where E : Exception
        {
            Action validationAction = () => stageDischargeReadingRecord.Validate();
            validationAction
                .ShouldThrow<E>()
                .And.Message.Should().Contain(propertyName);
        }

        private void SetValueToNull(ref StageDischargeReadingRecord stageDischargeReadingRecord, string propertyName)
        {
            var field = stageDischargeReadingRecord.GetType().GetField(propertyName);
            if (field != null)
            {
                field.SetValue(stageDischargeReadingRecord, null);
            }
        }

        [TestCase(nameof(StageDischargeReadingRecord.MeasurementId))]
        [TestCase(nameof(StageDischargeReadingRecord.ChannelWidth))]
        [TestCase(nameof(StageDischargeReadingRecord.ChannelArea))]
        [TestCase(nameof(StageDischargeReadingRecord.ChannelVelocity))]
        [TestCase(nameof(StageDischargeReadingRecord.Party))]
        [TestCase(nameof(StageDischargeReadingRecord.Comments))]
        public void StageDischargeRecord_SelfValidateWithNullableProperties_DoesNotThrow(string propertyName)
        {
            var stageDischargeRecord = StageDischargeCsvFileBuilder.CreateFullRecord(_fixture);
            CheckNoExceptionWhenSpecifiedFieldIsNull(stageDischargeRecord, propertyName);
        }

        private void CheckNoExceptionWhenSpecifiedFieldIsNull(StageDischargeReadingRecord stageDischargeReadingRecord, string propertyName)
        {
            SetValueToNull(ref stageDischargeReadingRecord, propertyName);
            Action validationAction = () => stageDischargeReadingRecord.Validate();
            validationAction.ShouldNotThrow();
        }


        [Test]
        public void StageDischargeRecord_SelfValidateWithAllRequiredValues_DoesNotThrow()
        {
            var stageDischargeRecord = StageDischargeCsvFileBuilder.CreateFullRecord(_fixture);
            Action noThrowAction = () => stageDischargeRecord.Validate();
            noThrowAction.ShouldNotThrow();
        }

        [Test]
        public void StageDischargeRecord_SelfValidate_WithStageAtStartButNoStageUnits_DetectsNull()
        {
            var stageDischargeRecord = StageDischargeCsvFileBuilder.CreateFullRecord(_fixture);

            stageDischargeRecord.StageUnits = null;
            stageDischargeRecord.StageAtEnd = null;

            CheckExpectedExceptionForProperty<ArgumentException>(stageDischargeRecord, nameof(stageDischargeRecord.StageAtStart));
        }

        [Test]
        public void StageDischargeRecord_SelfValidate_WithStageAtEndButNoStageUnits_DetectsNull()
        {
            var stageDischargeRecord = StageDischargeCsvFileBuilder.CreateFullRecord(_fixture);

            stageDischargeRecord.StageUnits = null;
            stageDischargeRecord.StageAtStart = null;

            CheckExpectedExceptionForProperty<ArgumentException>(stageDischargeRecord, nameof(stageDischargeRecord.StageAtEnd));
        }

        [Test]
        public void StageDischargeRecord_SelfValidate_Timestamps()
        {
            StageDischargeReadingRecord stageDischargeReadingRecord = StageDischargeCsvFileBuilder.CreateFullRecord(_fixture);
            Action validationAction = () => stageDischargeReadingRecord.Validate();
            stageDischargeReadingRecord.MeasurementStartDateTime = DateTimeOffset.Now;
            stageDischargeReadingRecord.MeasurementEndDateTime = stageDischargeReadingRecord.MeasurementStartDateTime;
            validationAction.ShouldNotThrow();

            stageDischargeReadingRecord.MeasurementEndDateTime = DateTimeOffset.Now.AddDays(1);
            validationAction.ShouldNotThrow();

            stageDischargeReadingRecord.MeasurementStartDateTime = DateTimeOffset.Now.AddDays(200);
            validationAction.ShouldThrow<ArgumentException>().And.Message.Should().Contain("MeasurementStartDateTime");
        }
    }
}
