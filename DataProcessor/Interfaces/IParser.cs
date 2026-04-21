using DataProcessor.Models;

namespace DataProcessor.Interfaces
{
    public interface IParser
    {
        Record Parse(string line);
    }
}