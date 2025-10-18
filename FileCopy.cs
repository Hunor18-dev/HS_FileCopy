using System;

namespace HS_FileCopy
{
    public class FileCopy
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;

        public FileCopy(string inputFilePath, string outputFilePath)
        {
            this._inputFilePath = inputFilePath;
            this._outputFilePath = outputFilePath;
        }
        public bool Copy()
        {
            this._displayCurrentTime();

            return true;
        }

        private void _displayCurrentTime()
        {
            Console.WriteLine($"Current time: {DateTime.Now.Millisecond}");
        }
    }
}
