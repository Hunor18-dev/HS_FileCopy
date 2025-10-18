using System;

namespace HS_FileCopy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started file copy...\n\n");

            string inputFilePath = "C:\\Users\\user\\Desktop\\Projects\\HornetSecurity_FileCopy\\FileCopyTest\\InputDir\\InputFile.bin";
            string outputFilePath = "C:\\Users\\user\\Desktop\\Projects\\HornetSecurity_FileCopy\\FileCopyTest\\OutputDir\\OutputFile.bin";

            /* at this point, the input file exists and the output file does not exist */
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
            
            Console.WriteLine($"\nFile copy completed. Success: {copyStatus}");
        }
    }
}
