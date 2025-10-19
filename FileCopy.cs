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

            this._splitAndCopyFileParallel(1); // 1 MB chunks

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

        private bool _splitAndCopyFileParallel(int chunkSizeMB, int parallelTasks = 2, int maxRetries = 3)
        {
            if (chunkSizeMB < 1) { chunkSizeMB = 1; }

            if (parallelTasks < 1) { parallelTasks = 1; }

            if (maxRetries < 1) { maxRetries = 1; }

            /* predefine the total chunks number */
            long fileLength = new FileInfo(this._inputFilePath).Length;
            long chunkSizeBytes = chunkSizeMB * 1024 * 1024;
            int numChunks = (int)Math.Ceiling((double)fileLength / chunkSizeBytes);

            Console.WriteLine($"Total number of chunks to be transfered: {numChunks}");

            var semaphore = new SemaphoreSlim(parallelTasks);
            FileHelper fileHelper = new FileHelper();
            
            /* create file so it can be used later */
            using (var outInit = new FileStream(_outputFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                outInit.SetLength(fileLength);
            }
            bool allSuccess = true;

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
                    while (!chunkCopied && attempt++ < maxRetries)
                    {
                        try
                        {
                            byte[] buffer = new byte[length];
                            int bytesRead = 0;

                            using (var sourceStream = new FileStream(_inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                sourceStream.Seek(offset, SeekOrigin.Begin);
                                bytesRead = sourceStream.Read(buffer, 0, (int)Math.Min(buffer.Length, length));
                            }
                            /* add bytesRead for the last chunk */
                            /* for chunk check use md5, sha256 will be used after assemble */
                            byte[] inputHash = fileHelper.GetHashMD5(buffer, bytesRead);

                            // Write chunk directly into output file at the correct position
                            using (FileStream destStream = new FileStream(_outputFilePath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                            {
                                destStream.Seek(offset, SeekOrigin.Begin);
                                destStream.Write(buffer, 0, bytesRead);
                            }

                            byte[] copiedBuffer = new byte[bytesRead];
                            using (FileStream partialDestStream = new FileStream(_outputFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                partialDestStream.Seek(offset, SeekOrigin.Begin);
                                int copiedChunk = partialDestStream.Read(copiedBuffer, 0, bytesRead);
                                if (copiedChunk != bytesRead)
                                {
                                    Console.WriteLine($"ChunkSizeError: Position: {i}: ChunkName: {chunkFileName}");
                                    continue;
                                }
                            }
                            byte[] targetHash = fileHelper.GetHashMD5(copiedBuffer, bytesRead);

                            bool hashesMatch = inputHash.SequenceEqual(targetHash);
                            string hashString = BitConverter.ToString(inputHash);

                            if (hashesMatch)
                            {
                                chunkCopied = true;
                                Console.WriteLine($"Position: {i}: Hash: {hashString}  - ChunkName: {chunkFileName}");
                            }
                            else
                            {
                                Console.WriteLine($"HashError: Position: {i}: ChunkName: {chunkFileName}");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Chunk {i:D4}, attempt {attempt} failed: {ex.Message}");
                        }
                    }

                    if (!chunkCopied)
                    {
                        allSuccess = false;
                        Console.WriteLine($"Chunk {i:D4} failed after {maxRetries} attempts.");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });
            return allSuccess;
        }

    }
}
