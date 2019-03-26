using System;
using System.Collections.Generic;
using System.Reflection;
using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.CrossSection;
using FieldDataPluginFramework.DataModel.DischargeActivities;
using FieldDataPluginFramework.DataModel.Readings;
using FieldDataPluginFramework.Results;

namespace StageDischargeReadings
{
    // The delayed appender class exists to solely to delay the creation of visits until merges have been resolved.
    // This will allow a visit to be created, then expanded (a widened Start or End timestamp) to include more activities.
    public class DelayedAppender : IDisposable, IFieldDataResultsAppender
    {
        public class InternalConstructor<T> where T : class
        {
            public static T Invoke(params object[] args)
            {
                return Activator.CreateInstance(typeof(T), BindingFlags.NonPublic | BindingFlags.Instance, null, args, null) as T;
            }
        }

        private IFieldDataResultsAppender ActualAppender { get; set;  }

        public DelayedAppender(IFieldDataResultsAppender actualAppender)
        {
            ActualAppender = actualAppender;
        }

        public void Dispose()
        {
            if (ActualAppender == null)
                return;

            AppendAllResults();
            ActualAppender = null;
        }

        private void AppendAllResults()
        {
            foreach (var delayedFieldVisit in DelayedFieldVisits)
            {
                AppendDelayedVisit(delayedFieldVisit);
            }
        }

        private void AppendDelayedVisit(FieldVisitInfo delayedVisit)
        {
            var visit = ActualAppender.AddFieldVisit(delayedVisit.LocationInfo, delayedVisit.FieldVisitDetails);

            foreach (var dischargeActivity in delayedVisit.DischargeActivities)
            {
                ActualAppender.AddDischargeActivity(visit, dischargeActivity);
            }

            foreach (var reading in delayedVisit.Readings)
            {
                ActualAppender.AddReading(visit, reading);
            }

            foreach (var crossSectionSurvey in delayedVisit.CrossSectionSurveys)
            {
                ActualAppender.AddCrossSectionSurvey(visit, crossSectionSurvey);
            }
        }

        public LocationInfo GetLocationByIdentifier(string locationIdentifier)
        {
            return ActualAppender.GetLocationByIdentifier(locationIdentifier);
        }

        public LocationInfo GetLocationByUniqueId(string uniqueId)
        {
            return ActualAppender.GetLocationByUniqueId(uniqueId);
        }

        private List<FieldVisitInfo> DelayedFieldVisits { get; } = new List<FieldVisitInfo>();

        public FieldVisitInfo AddFieldVisit(LocationInfo location, FieldVisitDetails fieldVisitDetails)
        {
            var fieldVisitInfo = InternalConstructor<FieldVisitInfo>.Invoke(location, fieldVisitDetails);

            DelayedFieldVisits.Add(fieldVisitInfo);

            return fieldVisitInfo;
        }

        public void AddDischargeActivity(FieldVisitInfo fieldVisit, DischargeActivity dischargeActivity)
        {
            fieldVisit.DischargeActivities.Add(dischargeActivity);
        }

        public void AddReading(FieldVisitInfo fieldVisit, Reading reading)
        {
            fieldVisit.Readings.Add(reading);
        }

        public void AddCrossSectionSurvey(FieldVisitInfo fieldVisit, CrossSectionSurvey crossSectionSurvey)
        {
            fieldVisit.CrossSectionSurveys.Add(crossSectionSurvey);
        }
    }
}
