using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using DataProcessor.Interfaces;
using DataProcessor.Models;

namespace DataProcessor.Implementations
{
    public class JsonExporter : IExporter
    {
        public void Export(IEnumerable<Record> records, string path)
        {
            var json = JsonSerializer.Serialize(records);
            File.WriteAllText(path, json);
        }
    }
}