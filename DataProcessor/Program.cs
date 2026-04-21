using System;
using DataProcessor.Implementations;
using DataProcessor.Services;

namespace DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new FileDataReader("input.csv");
            var parser = new CsvParser();
            var validator = new RecordValidator();
            var transformer = new RecordTransformer();
            var exporter = new JsonExporter();
            var logger = new FileLogger("log.txt");

            var processor = new DataProcessor(
                reader, parser, validator, transformer, exporter, logger
            );

            var records = processor.Process("output.json");

            var stats = new StatisticsService();
            stats.Calculate(records);

            Console.WriteLine($"Total Records: {stats.TotalRecords}");
            Console.WriteLine($"Average Value: {stats.AverageValue}");
        }
    }
}