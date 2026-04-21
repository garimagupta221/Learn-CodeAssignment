using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataProcessor.Interfaces;
using DataProcessor.Models;

namespace DataProcessor.Implementations
{
    public class XmlExporter : IExporter
    {
        public void Export(IEnumerable<Record> records, string path)
        {
            var serializer = new XmlSerializer(typeof(List<Record>));
            using var writer = new StreamWriter(path);
            serializer.Serialize(writer, new List<Record>(records));
        }
    }
}