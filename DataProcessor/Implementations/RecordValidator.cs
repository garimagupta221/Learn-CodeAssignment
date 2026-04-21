using DataProcessor.Interfaces;
using DataProcessor.Models;

namespace DataProcessor.Implementations
{
    public class RecordValidator : IValidator<Record>
    {
        public bool Validate(Record record, out string error)
        {
            if (string.IsNullOrWhiteSpace(record.Id))
            {
                error = "Missing ID";
                return false;
            }

            if (string.IsNullOrWhiteSpace(record.Name))
            {
                error = $"Record {record.Id} missing name";
                return false;
            }

            if (record.Value == 0)
            {
                error = $"Record {record.Id} has invalid value";
                return false;
            }

            error = null;
            return true;
        }
    }
}