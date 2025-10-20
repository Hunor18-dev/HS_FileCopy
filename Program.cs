using System;

namespace HS_FileCopy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\nStarted file copy...\n");

            if(args.Length != 2)
            {
                Console.WriteLine("Invalid number of arguments. Please provide input and output file paths.");
                return;
            }

            string inputFilePath = args[0];
            string outputFilePath = args[1];

            var fileCopy = new FileCopy(inputFilePath, outputFilePath);
            bool copyStatus = false;
            try
            {
                copyStatus = fileCopy.StartFileCopy();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\nError occurred in copying file. Error: {ex.Message}");
            }

            Console.WriteLine($"\nFile copy completed.");

            if (copyStatus)
            {
                Console.WriteLine("File has been copied successfully.");
            }
            else
            {
                Console.WriteLine("Failed to copy file.");
            } 
        }
    }
}
