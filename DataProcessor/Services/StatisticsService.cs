using System.Collections.Generic;
using System.Linq;
using DataProcessor.Models;

namespace DataProcessor.Services
{
    public class StatisticsService
    {
        public int TotalRecords { get; private set; }
        public double TotalValue { get; private set; }
        public double AverageValue { get; private set; }

        public void Calculate(IEnumerable<Record> records)
        {
            var list = records.ToList();
            TotalRecords = list.Count;
            TotalValue = list.Sum(record => record.Value);
            AverageValue = list.Any() ? list.Average(record => record.Value) : 0;
        }
    }
}