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
        public bool StartFileCopy()
        {
            var fileHelper = new FileHelper();
            (bool inputExists, long inputSize) = fileHelper.FileExists(this._inputFilePath);
            (bool outputExists, long outputSize) = fileHelper.FileExists(this._outputFilePath);
            Console.WriteLine($"Input File - Exists: {inputExists}, Size: {inputSize} bytes\n");
            Console.WriteLine($"Output File - Exists: {outputExists}, Size: {outputSize} bytes\n");

            if (!inputExists)
            {
                Console.WriteLine("Input file does not exist! Aborting copy.");
                return false;
            }

            if (outputExists)
            {
                Console.WriteLine("Output file already exists! Output file will be overwritten.");
                fileHelper.DeleteFile(this._outputFilePath);
            }

            this._displayCurrentTime();
            File.Copy(this._inputFilePath, this._outputFilePath);
            return true;
        }

        private void _displayCurrentTime()
        {
            Console.WriteLine($"Current time: {DateTime.Now.Millisecond}");
        }
    }
}
