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

            int startTime = DateTime.Now.Millisecond;

            if (!this._verifyFiles())
            {
                return false;
            }


            bool copyStatus = this._copyFile();

            int endTime = DateTime.Now.Millisecond;
            Console.WriteLine($"File copy took {endTime - startTime} milliseconds.");

            return copyStatus;
        }

        private bool _verifyFiles()
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
            return true;
        }

        private bool _copyFile()
        {

            File.Copy(this._inputFilePath, this._outputFilePath);
            if (!this._verifyCopy())
            {
                Console.WriteLine("File copy verification failed!");
                return false;
            }
            return true;

        }

        private bool _verifyCopy()
        {
            var fileHelper = new FileHelper();
            (bool inputExists, long inputSize) = fileHelper.FileExists(this._inputFilePath);
            (bool outputExists, long outputSize) = fileHelper.FileExists(this._outputFilePath);

            if (inputExists && outputExists && inputSize == outputSize)
            {
                byte[] inputChecksum = fileHelper.GetChecksum(this._inputFilePath);
                byte[] outputChecksum = fileHelper.GetChecksum(this._outputFilePath);
                return inputChecksum.SequenceEqual(outputChecksum);
            }
            return false;
        }
    }
}
