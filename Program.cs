using System;

namespace HS_FileCopy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started file copy...\n");

            string inputFilePath = "C:\\path\\to\\source\\file.txt";
            string outputFilePath = "C:\\path\\to\\destination\\file.txt";

            var fileHelper = new FileHelper();
            fileHelper.FileExists(inputFilePath);
            fileHelper.FileExists(outputFilePath);

            var fileCopy = new FileCopy(inputFilePath, outputFilePath);
            
            fileCopy.Copy();

            Console.WriteLine("\nFile copy completed.");
        }
    }
}
