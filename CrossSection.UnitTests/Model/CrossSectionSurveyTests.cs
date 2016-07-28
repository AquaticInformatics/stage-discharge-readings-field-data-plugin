﻿using System;
using NUnit.Framework;
using Server.BusinessInterfaces.FieldDataPlugInCore.Exceptions;
using Server.Plugins.FieldVisit.CrossSection.Model;

namespace Server.Plugins.FieldVisit.CrossSection.UnitTests.Model
{
    [TestFixture]
    public class CrossSectionSurveyTests
    {
        [Test]
        public void GetFieldValue_NullField_Throws()
        {
            var crossSection = new CrossSectionSurvey();

            TestDelegate testDelegate = () => crossSection.GetFieldValue(null);

            Assert.That(testDelegate, Throws.Exception.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void GetFieldValue_FieldDoesNotExist_Throws()
        {
            var crossSection = new CrossSectionSurvey();

            const string field = "SomeKey";

            TestDelegate testDelegate = () => crossSection.GetFieldValue(field);

            Assert.That(testDelegate, Throws.Exception.TypeOf<ParsingFailedException>().With.Message.Contains(field));
        }

        [Test]
        public void GetFieldValue_FieldExists_ReturnsExpectedValue()
        {
            var crossSection = new CrossSectionSurvey();

            const string field = "SomeKey";
            const string data = "value";
            crossSection.Fields.Add(field, data);

            var result = crossSection.GetFieldValue(field);

            Assert.That(result, Is.EqualTo(data));
        }
    }
}
