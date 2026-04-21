using System.Collections.Generic;

namespace DataProcessor.Interfaces
{
    public interface IDataReader
    {
        IEnumerable<string> Read();
    }
}