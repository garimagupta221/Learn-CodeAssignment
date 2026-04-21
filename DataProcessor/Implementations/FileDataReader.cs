using System.Collections.Generic;
using System.IO;
using DataProcessor.Interfaces;

namespace DataProcessor.Implementations
{
    public class FileDataReader : IDataReader
    {
        public FileDataReader(string filePath)
        {
            _filePath = filePath;
        }

        public IEnumerable<string> Read()
        {
            return File.ReadLines(_filePath);
        }

        private readonly string _filePath;
    }
}