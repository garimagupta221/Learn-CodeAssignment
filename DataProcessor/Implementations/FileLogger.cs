using System;
using System.IO;
using DataProcessor.Interfaces;

namespace DataProcessor.Implementations
{
    public class FileLogger : ILogger
    {
        public FileLogger(string path)
        {
            _path = path;
        }

        public void Log(string message)
        {
            var log = $"[{DateTime.Now}] {message}";
            File.AppendAllText(_path, log + Environment.NewLine);
        }

        private readonly string _path;
    }
}