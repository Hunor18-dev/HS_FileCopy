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
            if (!this._verifyFiles())
            {
                return false;
            }


            // bool copyStatus = this._copyFile();
            this._splitAndCopyFileParallel(1); // 1 MB chunks
            this._assembleChunks();
            this._cleanUpChunks();
            bool copyStatus = this._verifyCopy();

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
                /* for final check use sha256 */
                byte[] inputChecksum = fileHelper.GetHashSHA256(this._inputFilePath);
                byte[] outputChecksum = fileHelper.GetHashSHA256(this._outputFilePath);
                return inputChecksum.SequenceEqual(outputChecksum);
            }
            return false;
        }

        private void _splitAndCopyFileLinear(int chunkSizeMB)
        {
            FileHelper fileHelper = new FileHelper();

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

                // Write chunk
                using (FileStream chunkStream = new FileStream(targetChunkPath, FileMode.Create, FileAccess.Write))
                {
                    /* keep bytesRead, might not be exact size */
                    chunkStream.Write(buffer, 0, bytesRead);
                }

                /* add bytesRead for the last chunk */
                /* for chunck check use md5, sha256 will be used after assemble */
                byte[] inputChecksum = fileHelper.GetHashMD5(buffer, bytesRead);
                byte[] targetChecksum = fileHelper.GetHashMD5(targetChunkPath);
                bool checksumsMatch = inputChecksum.SequenceEqual(targetChecksum);
                string checkSumString = BitConverter.ToString(inputChecksum);

                if (checksumsMatch)
                {
                    Console.WriteLine($"Position: {index}: Hash: {checkSumString}  - ChunkName: {chunkFileName}");
                }
                else
                {
                    Console.WriteLine($"HashError: Position: {index}: ChunkName: {chunkFileName}");
                }

                index++;
            }
        }

        private bool _splitAndCopyFileParallel(int chunkSizeMB, int parallelTasks = 2, int maxRetries = 3)
        {
            if (chunkSizeMB < 1)
            {
                chunkSizeMB = 1;
            }

            if (parallelTasks < 1)
            {
                parallelTasks = 1;
            }

            if (maxRetries < 1)
            {
                maxRetries = 1;
            }

            /* predefine the total chunks number */
            long fileLength = new FileInfo(this._inputFilePath).Length;
            long chunkSizeBytes = chunkSizeMB * 1024 * 1024;
            int numChunks = (int)Math.Ceiling((double)fileLength / chunkSizeBytes);

            Console.WriteLine($"Total number of chunks to be transfered: {numChunks}");

            var semaphore = new SemaphoreSlim(parallelTasks);
            FileHelper fileHelper = new FileHelper();
            Parallel.For(0, numChunks, i =>
            {
                semaphore.Wait();
                try
                {
                    long offset = i * chunkSizeBytes;
                    /* handle last chunk size */
                    long length = Math.Min(chunkSizeBytes, fileLength - offset);
                    string chunkFileName = $"chunk_{i:D4}.part";
                    string targetChunkPath = Path.Combine(_outputDirectory, chunkFileName);

                    bool chunkCopied = false;
                    int attempt = 0;
                    while(!chunkCopied && attempt++ < maxRetries)
                    {
                        try
                        {
                            using (var sourceStream = new FileStream(_inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                sourceStream.Seek(offset, SeekOrigin.Begin);
                                byte[] buffer = new byte[length];

                                int bytesRead = sourceStream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));

                                // Write chunk
                                using (FileStream chunkStream = new FileStream(targetChunkPath, FileMode.Create, FileAccess.Write))
                                {
                                    /* keep bytesRead, might not be exact size */
                                    chunkStream.Write(buffer, 0, bytesRead);
                                }
                                /* add bytesRead for the last chunk */
                                /* for chunck check use md5, sha256 will be used after assemble */
                                byte[] inputChecksum = fileHelper.GetHashMD5(buffer, bytesRead);
                                byte[] targetChecksum = fileHelper.GetHashMD5(targetChunkPath);
                                bool checksumsMatch = inputChecksum.SequenceEqual(targetChecksum);
                                string checkSumString = BitConverter.ToString(inputChecksum);

                                if (checksumsMatch)
                                {
                                    chunkCopied = true;
                                    Console.WriteLine($"Position: {i}: Hash: {checkSumString}  - ChunkName: {chunkFileName}");
                                }
                                else
                                {
                                    Console.WriteLine($"HashError: Position: {i}: ChunkName: {chunkFileName}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Chunk {i:D4}, attempt {attempt} failed: {ex.Message}");
                        }
                    }

                    if (!chunkCopied)
                    {
                        throw new IOException($"Chunk {i:D4} failed after {maxRetries} attempts.");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
            return true;
        }

        private void _assembleChunks()
        {

            string[] chunkFiles = Directory.GetFiles(this._outputDirectory, "chunk_*.part");
            Array.Sort(chunkFiles);

            using FileStream outputStream = new FileStream(this._outputFilePath!, FileMode.Create, FileAccess.Write);

            FileHelper fileHelper = new FileHelper();

            foreach (string chunkFile in chunkFiles)
            {
                using (FileStream chunkStream = new FileStream(chunkFile, FileMode.Open, FileAccess.Read))
                {
                    chunkStream.CopyTo(outputStream);   
                }
                
                fileHelper.DeleteFile(chunkFile);
            }
        }
        
        private void _cleanUpChunks()
        {
            string[] chunkFiles = Directory.GetFiles(this._outputDirectory, "chunk_*.part");

            FileHelper fileHelper = new FileHelper();

            foreach (string chunkFile in chunkFiles)
            {
                fileHelper.DeleteFile(chunkFile);
            }
        }
    }
}
