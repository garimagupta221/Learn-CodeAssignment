using System.Collections.Generic;
using DataProcessor.Interfaces;
using DataProcessor.Models;

namespace DataProcessor.Services
{
    public class DataProcessor
    {
        public DataProcessor(
            IDataReader reader,
            IParser parser,
            IValidator<Record> validator,
            ITransformer<Record> transformer,
            IExporter exporter,
            ILogger logger)
        {
            _reader = reader;
            _parser = parser;
            _validator = validator;
            _transformer = transformer;
            _exporter = exporter;
            _logger = logger;
        }

        public List<Record> Process(string outputPath)
        {
            var records = new List<Record>();

            foreach (var line in _reader.Read())
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                var record = _parser.Parse(line);

                if (!_validator.Validate(record, out var error))
                {
                    _logger.Log(error);
                    continue;
                }

                records.Add(_transformer.Transform(record));
            }

            _exporter.Export(records, outputPath);
            return records;
        }

        public List<Record> FilterByValue(List<Record> records, double minValue)
        {
            return records.FindAll(record => record.Value >= minValue);
        }

        private readonly IDataReader _reader;
        private readonly IParser _parser;
        private readonly IValidator<Record> _validator;
        private readonly ITransformer<Record> _transformer;
        private readonly IExporter _exporter;
        private readonly ILogger _logger;
    }
}