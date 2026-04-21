using System;
using DataProcessor.Interfaces;
using DataProcessor.Models;

namespace DataProcessor.Implementations
{
    public class CsvParser : IParser
    {
        public Record Parse(string line)
        {
            var parts = line.Split(',');

            return new Record
            {
                Id = parts[0].Trim(),
                Name = parts[1].Trim(),
                Value = double.TryParse(parts[2], out var val) ? val : 0,
                Date = parts.Length > 3 && DateTime.TryParse(parts[3], out var date) ? date : null
            };
        }
    }
}