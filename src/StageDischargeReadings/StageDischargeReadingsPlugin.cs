using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FieldDataPluginFramework;
using FieldDataPluginFramework.Context;
using FieldDataPluginFramework.DataModel;
using FieldDataPluginFramework.DataModel.DischargeActivities;
using FieldDataPluginFramework.Results;
using StageDischargeReadings.Interfaces;
using StageDischargeReadings.Mappers;
using StageDischargeReadings.Parsers;

namespace StageDischargeReadings
{
    public class StageDischargeReadingsPlugin : IFieldDataPlugin
    {
        public const string NoRecordsInInputFile = "No records found in input file.";
        public const string InputFileContainsInvalidRecords = "Input file contains invalid records.";
        private readonly IDataParser<StageDischargeReadingRecord> _parser;
        private IFieldDataResultsAppender _fieldDataResultsAppender;
        private readonly DischargeActivityMapper _dischargeActivityMapper;
        private ILog _log;

        public StageDischargeReadingsPlugin() : this(new CsvDataParser<StageDischargeReadingRecord>())
        {}

        public StageDischargeReadingsPlugin(IDataParser<StageDischargeReadingRecord> parser)
        {
            _parser = parser;
            _dischargeActivityMapper = new DischargeActivityMapper();
        }

        public ParseFileResult ParseFile(Stream fileStream, IFieldDataResultsAppender fieldDataResultsAppender, ILog logger)
        {
            _log = logger;

            try
            {
                using (var delayedAppender = new DelayedAppender(fieldDataResultsAppender))
                {
                    _fieldDataResultsAppender = delayedAppender;

                    var parsedRecords = _parser.ParseInputData(fileStream);
                    if (parsedRecords == null)
                    {
                        return ParseFileResult.CannotParse(NoRecordsInInputFile);
                    }

                    if (_parser.Errors.Any())
                    {
                        if (_parser.ValidRecords > 0)
                        {
                            return ParseFileResult.SuccessfullyParsedButDataInvalid(
                                $"{InputFileContainsInvalidRecords}: {_parser.Errors.Length} errors:\n{string.Join("\n", _parser.Errors.Take(3))}");
                        }

                        return ParseFileResult.CannotParse();
                    }

                    _log.Info($"Parsed {_parser.ValidRecords} rows from input file.");
                    SaveRecords(parsedRecords);
                    return ParseFileResult.SuccessfullyParsedAndDataValid();
                }
            }
            catch (Exception e)
            {
                _log.Error($"Failed to parse file; {e.Message}\n{e.StackTrace}");
                return ParseFileResult.CannotParse(e);
            }
        }

        private void SaveRecords(IEnumerable<StageDischargeReadingRecord> parsedRecords)
        {
            var sortedRecordsByLocation = parsedRecords
                .GroupBy(r => r.LocationIdentifier)
                .ToDictionary(r => _fieldDataResultsAppender.GetLocationByIdentifier(r.Key),
                              v => v.OrderBy(x => x.MeasurementStartDateTime).ToList());

            foreach (var locationInfo in sortedRecordsByLocation.Keys.OrderBy(l => l.LocationIdentifier))
            {
                CreateVisitsAndActivities(locationInfo, sortedRecordsByLocation[locationInfo]);
            }
        }

        private List<FieldVisitInfo> CreatedVisits { get; set; }
        private Dictionary<string,FieldVisitInfo> VisitsByMeasurementId { get; set; }

        private void CreateVisitsAndActivities(LocationInfo location, IEnumerable<StageDischargeReadingRecord> locationRecords)
        {
            CreatedVisits = new List<FieldVisitInfo>();
            VisitsByMeasurementId = new Dictionary<string, FieldVisitInfo>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var stageDischargeRecord in locationRecords)
            {
                var fieldVisit = MergeOrCreateVisit(location, stageDischargeRecord);

                CreateDischargeActivityForVisit(fieldVisit, stageDischargeRecord);
                CreateReadingsForVisit(fieldVisit, stageDischargeRecord);
            }

            var midnightVisits = CreatedVisits
                .Where(fv => fv.StartDate.Date < fv.EndDate.Date)
                .ToList();

            _log.Info($"{location.LocationIdentifier} created {CreatedVisits.Count} visits.");

            if (midnightVisits.Any())
            {
                _log.Info($"{location.LocationIdentifier} had {midnightVisits.Count} visits spanning midnight: {string.Join(", ", midnightVisits.Select(fv => $"Start={fv.StartDate} End={fv.EndDate}"))}");
            }
        }

        private FieldVisitInfo MergeOrCreateVisit(LocationInfo location, StageDischargeReadingRecord visitRecord)
        {
            var existingVisit = FindExistingVisit(visitRecord);

            if (existingVisit != null)
            {
                MergeWithExistingVisit(existingVisit, visitRecord);

                return existingVisit;
            }

            var visitStart = visitRecord.MeasurementStartDateTime;
            var visitEnd = visitRecord.MeasurementEndDateTime;

            var fieldVisitDetails = new FieldVisitDetails(new DateTimeInterval(visitStart, visitEnd))
            {
                Comments = visitRecord.Comments,
                Party = visitRecord.Party
            };

            var fieldVisitInfo = _fieldDataResultsAppender.AddFieldVisit(location, fieldVisitDetails);

            CreatedVisits.Add(fieldVisitInfo);

            if (IsValidMeasurementId(visitRecord.MeasurementId))
            {
                AddVisitByMeasurementId(fieldVisitInfo, visitRecord);
            }

            return fieldVisitInfo;
        }

        private static bool IsValidMeasurementId(string measurementId)
        {
            return !string.IsNullOrWhiteSpace(measurementId)
                   && measurementId.Trim() != "0"; // 0 is a commonly used, but bad, measurement ID from 3.X
        }

        private FieldVisitInfo FindExistingVisit(StageDischargeReadingRecord visitRecord)
        {
            if (VisitsByMeasurementId.TryGetValue(visitRecord.MeasurementId.Trim(), out var otherVisit))
            {
                if (otherVisit.StartDate.Date == visitRecord.MeasurementStartDateTime.Date
                    || otherVisit.EndDate.Date == visitRecord.MeasurementEndDateTime.Date)
                {
                    // We can match up visits by measurement ID
                    _log.Info($"Found associated measurementId={visitRecord.MeasurementId} at Start={otherVisit.StartDate} and End={otherVisit.EndDate}");
                    return otherVisit;
                }
            }

            var containingVisits = CreatedVisits
                .Where(fv => fv.StartDate <= visitRecord.MeasurementStartDateTime
                             && fv.EndDate >= visitRecord.MeasurementEndDateTime)
                .ToList();

            if (containingVisits.Count == 1)
            {
                // The record fits completely within exactly one existing visit
                var visit = containingVisits.Single();
                _log.Info($"Merging existing {visit.LocationInfo.LocationIdentifier} visit.Start={visit.StartDate} visit.End={visit.EndDate} with completely contained record.Start={visitRecord.MeasurementStartDateTime} record.End={visitRecord.MeasurementEndDateTime}");
                return visit;
            }

            var possibleVisits = CreatedVisits
                .Where(fv => fv.StartDate.Date == visitRecord.MeasurementStartDateTime.Date
                             || fv.EndDate.Date == visitRecord.MeasurementEndDateTime.Date)
                .ToList();

            if (possibleVisits.Count == 1)
            {
                // An exact match with on existing visit on the same day as the record
                var visit = possibleVisits.Single();
                _log.Info($"Merging existing {visit.LocationInfo.LocationIdentifier} visit.Start={visit.StartDate} visit.End={visit.EndDate} with same day record.Start={visitRecord.MeasurementStartDateTime} record.End={visitRecord.MeasurementEndDateTime}");
                return visit;
            }

            if (possibleVisits.Count == 0)
                return null;

            var error = $"Confused merge of record.Start={visitRecord.MeasurementStartDateTime} record.End={visitRecord.MeasurementEndDateTime} with {possibleVisits.Count} possible visits: {string.Join(", ", possibleVisits.Select(fv => $"Start={fv.StartDate} End={fv.EndDate}"))}";

            _log.Error(error);
            throw new Exception(error);
        }

        private void MergeWithExistingVisit(FieldVisitInfo existingVisit, StageDischargeReadingRecord visitRecord)
        {
            existingVisit.FieldVisitDetails.FieldVisitPeriod = ExpandInterval(
                existingVisit.FieldVisitDetails.FieldVisitPeriod,
                visitRecord.MeasurementStartDateTime,
                visitRecord.MeasurementEndDateTime);

            existingVisit.FieldVisitDetails.Comments =
                MergeUniqueComments(existingVisit.FieldVisitDetails.Comments, visitRecord.Comments);

            existingVisit.FieldVisitDetails.Party =
                MergeUniqueParties(existingVisit.FieldVisitDetails.Party, visitRecord.Party);

            var visitDuration = existingVisit.FieldVisitDetails.FieldVisitPeriod.End -
                                existingVisit.FieldVisitDetails.FieldVisitPeriod.Start;

            if (visitDuration.TotalHours > 36)
            {
                // Log a warning, but keep going
                _log.Error($"{existingVisit.LocationInfo.LocationIdentifier} visit Start={existingVisit.FieldVisitDetails.FieldVisitPeriod.Start} End={existingVisit.FieldVisitDetails.FieldVisitPeriod.End} exceeds 36 hours TotalHours={visitDuration.TotalHours:F1}");
            }

            if (!IsValidMeasurementId(visitRecord.MeasurementId))
                return;

            AddVisitByMeasurementId(existingVisit, visitRecord);
        }

        private void AddVisitByMeasurementId(FieldVisitInfo fieldVisitInfo, StageDischargeReadingRecord visitRecord)
        {
            var measurementId = visitRecord.MeasurementId.Trim();

            if (VisitsByMeasurementId.ContainsKey(measurementId))
            {
                var associatedVisit = VisitsByMeasurementId[measurementId];

                if (associatedVisit != fieldVisitInfo)
                {
                    _log.Error($"MeasurementId={measurementId} is already associated with a different {associatedVisit.LocationInfo.LocationIdentifier} visit Start={associatedVisit.StartDate} End={associatedVisit.EndDate}. Can't switch to a different {fieldVisitInfo.LocationInfo.LocationIdentifier} visit Start={fieldVisitInfo.StartDate} End={fieldVisitInfo.EndDate}");

                    // Re-associate it, but the import is likely going to fail anyway.
                    VisitsByMeasurementId[measurementId] = fieldVisitInfo;
                }

                return;
            }

            VisitsByMeasurementId.Add(measurementId, fieldVisitInfo);
        }

        private static string MergeUniqueComments(params string[] values)
        {
            return MergeUniqueStrings("\n", values);
        }

        private static string MergeUniqueParties(params string[] values)
        {
            return MergeUniqueStrings(", ", values);
        }

        private static string MergeUniqueStrings(string separator, string[] values)
        {
            var distinctValues = values
                .SelectMany(s => s.Split(new []{separator}, StringSplitOptions.RemoveEmptyEntries))
                .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

            return string.Join(separator, distinctValues);
        }

        private static DateTimeInterval ExpandInterval(DateTimeInterval interval, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var minStart = interval.Start < startTime
                ? interval.Start
                : startTime;

            var maxEnd = interval.End > endTime
                ? interval.End
                : endTime;

            return new DateTimeInterval(minStart, maxEnd);
        }

        private void CreateDischargeActivityForVisit(FieldVisitInfo fieldVisit, StageDischargeReadingRecord record)
        {
            if (!record.Discharge.HasValue) return;

            _fieldDataResultsAppender.AddDischargeActivity(fieldVisit, CreateDischargeActivityFromRecord(record));
        }

        private DischargeActivity CreateDischargeActivityFromRecord(StageDischargeReadingRecord record)
        {
            return _dischargeActivityMapper.FromStageDischargeRecord(record);
        }

        private void CreateReadingsForVisit(FieldVisitInfo fieldVisit, StageDischargeReadingRecord record)
        {
            foreach (var reading in record.Readings)
            {
                _fieldDataResultsAppender.AddReading(fieldVisit, reading);
            }
        }

        public ParseFileResult ParseFile(Stream fileStream, LocationInfo selectedLocation, IFieldDataResultsAppender fieldDataResultsAppender,
            ILog logger)
        {
            return ParseFile(fileStream, fieldDataResultsAppender, logger);
        }
    }
}
