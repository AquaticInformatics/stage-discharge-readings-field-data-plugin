using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using FileHelpers;
using StageDischargeReadings.Interfaces;

namespace StageDischargeReadings.UnitTests.Helpers
{
    internal class InMemoryCsvFile<TRecordType> where TRecordType : class, ISelfValidator
    {
        private readonly List<TRecordType> _records;

        public InMemoryCsvFile()
        {
            _records = new List<TRecordType>();
        }

        public void AddRecord(TRecordType record)
        {
            _records.Add(record);
        }

        public MemoryStream GetInMemoryCsvFileStream()
        {
            MemoryStream theMemStream = new MemoryStream();
            TextWriter writer = new StreamWriter(theMemStream, Encoding.UTF8, 512, true);
            var engine = new FileHelperAsyncEngine<TRecordType>();
            engine.HeaderText = engine.GetFileHeader();
            engine.BeginWriteStream(writer);
            foreach (TRecordType record in _records)
            {
                engine.WriteNext(record);
            }
            engine.Flush();
            theMemStream.Seek(0, SeekOrigin.Begin);
            StreamToStringToConsole(theMemStream);
            return theMemStream;
        }

        [Conditional("DEBUG")]
        public void StreamToStringToConsole(Stream stream)
        {
            long position = stream.Position;
            string data = new StreamReader(stream).ReadToEnd();
            Console.WriteLine(data);
            stream.Seek(position, SeekOrigin.Begin);
        }
    }
}
