using System;

namespace HS_FileCopy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started file copy...\n\n");

            string inputFilePath = "C:\\path\\to\\source\\file.txt";
            string outputFilePath = "C:\\path\\to\\destination\\file.txt";

            var fileHelper = new FileHelper();
            (bool inputExists, long inputSize) = fileHelper.FileExists(inputFilePath);
            (bool outputExists, long outputSize) = fileHelper.FileExists(outputFilePath);
            Console.WriteLine($"Input File - Exists: {inputExists}, Size: {inputSize} bytes\n");
            Console.WriteLine($"Output File - Exists: {outputExists}, Size: {outputSize} bytes\n");

            if (!inputExists)
            {
                Console.WriteLine("Input file does not exist! Aborting copy.");
                return;
            }

            if (outputExists)
            {
                Console.WriteLine("Output file already exists! Output file will be overwritten.");
                fileHelper.DeleteFile(outputFilePath);
            }
            
            /* at this point, the input file exists and the output file does not exist */
            var fileCopy = new FileCopy(inputFilePath, outputFilePath);

            bool copyStatus = fileCopy.Copy();

            Console.WriteLine($"\nFile copy completed. Success: {copyStatus}");
        }
    }
}
