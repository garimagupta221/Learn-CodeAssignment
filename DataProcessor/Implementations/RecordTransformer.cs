using DataProcessor.Interfaces;
using DataProcessor.Models;

namespace DataProcessor.Implementations
{
    public class RecordTransformer : ITransformer<Record>
    {
        public Record Transform(Record record)
        {
            record.Name = record.Name.ToUpper();
            return record;
        }
    }
}