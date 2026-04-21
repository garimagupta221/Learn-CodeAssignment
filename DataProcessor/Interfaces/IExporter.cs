using System.Collections.Generic;
using DataProcessor.Models;

namespace DataProcessor.Interfaces
{
    public interface IExporter
    {
        void Export(IEnumerable<Record> records, string path);
    }
}