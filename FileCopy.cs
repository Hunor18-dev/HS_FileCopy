using System;

namespace HS_FileCopy
{
    public class FileCopy
    {
        private readonly string _inputFilePath;
        private readonly string _outputFilePath;

        private readonly string _inputDirectory;
        private readonly string _outputDirectory;

        public FileCopy(string inputFilePath, string outputFilePath)
        {
            ArgumentNullException.ThrowIfNull(inputFilePath);
            ArgumentNullException.ThrowIfNull(outputFilePath);

            this._inputFilePath = inputFilePath;
            this._outputFilePath = outputFilePath;
            this._inputDirectory = Path.GetDirectoryName(this._inputFilePath!);
            this._outputDirectory = Path.GetDirectoryName(this._outputFilePath!);
        }
        public bool StartFileCopy()
        {

            int startTime = DateTime.Now.Millisecond;

            if (!this._verifyFiles())
            {
                return false;
            }


            // bool copyStatus = this._copyFile();
            this.SplitAndCopyFile(1); // 1 MB chunks
            this._assemlbeChunks();
            bool copyStatus = this._verifyCopy();

            int endTime = DateTime.Now.Millisecond;
            Console.WriteLine($"File copy took {endTime - startTime} milliseconds.");

            return copyStatus;
        }

        private bool _verifyFiles()
        {
            var fileHelper = new FileHelper();
            (bool inputExists, long inputSize) = fileHelper.FileExists(this._inputFilePath);
            (bool outputExists, long outputSize) = fileHelper.FileExists(this._outputFilePath);
            bool outputDirExists = fileHelper.DirectoryExists(this._outputDirectory!);
            Console.WriteLine($"Input File - Exists: {inputExists}, Size: {inputSize} bytes\n");
            Console.WriteLine($"Output File - Exists: {outputExists}, Size: {outputSize} bytes\n");

            if (!inputExists)
            {
                Console.WriteLine("Input file does not exist! Aborting copy.");
                return false;
            }

            if (!outputDirExists)
            {
                Console.WriteLine("Output directory does not exist! Aborting copy.");
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

        private void SplitAndCopyFile(int chunkSizeMB)
        {

            int chunkSizeBytes = chunkSizeMB * 1024 * 1024;
            byte[] buffer = new byte[chunkSizeBytes];

            using FileStream sourceStream = new FileStream(this._inputFilePath!, FileMode.Open, FileAccess.Read);
            int index = 0;
            int bytesRead;

            Console.WriteLine($"Splitting {this._inputFilePath} into {chunkSizeMB}MB chunks...");

            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string chunkFileName = $"chunk_{index:D4}.part";
                string targetChunkPath = Path.Combine(_outputDirectory, chunkFileName);

                // Write chunk locally
                using (FileStream chunkStream = new FileStream(targetChunkPath, FileMode.Create, FileAccess.Write))
                {
                    /* keep bytesRead, might not be exact size */
                    chunkStream.Write(buffer, 0, bytesRead);
                }
                index++;
            }
        }

        private void _assemlbeChunks()
        {
            string[] chunkFiles = Directory.GetFiles(this._outputDirectory, "chunk_*.part");
            Array.Sort(chunkFiles);

            using FileStream outputStream = new FileStream(this._outputFilePath!, FileMode.Create, FileAccess.Write);

            foreach (string chunkFile in chunkFiles)
            {
                using FileStream chunkStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read);
                chunkStream.CopyTo(outputStream);
            }
        }
    }
}
