﻿using System;
using System.Collections.Generic;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.ChannelMeasurements;
using Framework = FieldDataPluginFramework.DataModel.CrossSection;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Server.Plugins.FieldVisit.CrossSection.Interfaces;
using Server.Plugins.FieldVisit.CrossSection.Mappers;
using Server.Plugins.FieldVisit.CrossSection.UnitTests.TestData;
using CrossSectionPoint = Server.Plugins.FieldVisit.CrossSection.Model.CrossSectionPoint;
using CrossSectionSurvey = Server.Plugins.FieldVisit.CrossSection.Model.CrossSectionSurvey;

namespace Server.Plugins.FieldVisit.CrossSection.UnitTests.Mappers
{
    [TestFixture]
    public class CrossSectionMapperTests
    {
        private ICrossSectionMapper _crossSectionMapper;

        private ICrossSectionPointMapper _mockCrossSectionPointMapper;
        private CrossSectionSurvey _crossSectionSurvey;

        [SetUp]
        public void TestSetup()
        {
            _mockCrossSectionPointMapper = Substitute.For<ICrossSectionPointMapper>();
            _mockCrossSectionPointMapper.MapPoints(Arg.Any<List<CrossSectionPoint>>())
                .Returns(new List<Framework.CrossSectionPoint>());

            _crossSectionMapper = new CrossSectionMapper(_mockCrossSectionPointMapper);

            _crossSectionSurvey = new CrossSectionSurvey
            {
                Fields = TestHelpers.CreateExpectedCrossSectionFields()
            };
        }

        [Test]
        public void MapCrossSection_CrossSectionWithExpectedMetadata_CreatesExpectedCrossSectionSurvey()
        {
            var expectedCrossSectionSurvey = CreateExpectedCrossSectionSurvey();

            var actual = _crossSectionMapper.MapCrossSection(_crossSectionSurvey);

            actual.ShouldBeEquivalentTo(expectedCrossSectionSurvey);
        }

        private static Framework.CrossSectionSurvey CreateExpectedCrossSectionSurvey()
        {
            var startTime = new DateTimeOffset(2001, 05, 08, 14, 32, 15, TimeSpan.FromHours(7));
            var endTime = new DateTimeOffset(2001, 05, 08, 17, 12, 45, TimeSpan.FromHours(7));
            var surveyPeriod = new DateTimeInterval(startTime, endTime);

            var newCrossSectionSurvey =
                new Framework.CrossSectionSurvey(surveyPeriod, "Right overflow",  "At the Gage", "ft", StartPointType.LeftEdgeOfWater)
                {
                    Party = "Cross-Section Party",
                    Comments = "Cross-section survey comments",
                    StageMeasurement = new Measurement(12.2, "ft"),

                };

            newCrossSectionSurvey.Points(new List<Framework.CrossSectionPoint>());

            return newCrossSectionSurvey;
        }

        [Test]
        public void MapCrossSection_CrossSectionPoints_CallsMapPointsOnCrossSectionPointMapper()
        {
            _crossSectionMapper.MapCrossSection(_crossSectionSurvey);

            _mockCrossSectionPointMapper.Received(1).MapPoints(Arg.Any<List<CrossSectionPoint>>());
        }

        [TestCase("Unit")]
        [TestCase("Stage")]
        public void MapCrossSection_MissingRequiredHeaderProperty_ThrowsException(string propertyName)
        {
            _crossSectionSurvey.Fields.Remove(propertyName);

            void TestDelegate() => _crossSectionMapper.MapCrossSection(_crossSectionSurvey);

            Assert.That(TestDelegate, Throws.Exception.With.Message.Contains(propertyName));
        }
    }
}
