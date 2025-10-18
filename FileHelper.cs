using System;

namespace HS_FileCopy
{
    public class FileHelper
    {
        public (bool, long) FileExists(string filePat)
        {
            long fileSizeBytes = 0;
            bool exists = File.Exists(filePat);
            if (exists)
            {
                fileSizeBytes = new FileInfo(filePat).Length;
            }
            return (exists, fileSizeBytes);
        }

        public void DeleteFile(string filePath)
        {
            File.Delete(filePath);
        }


    }
}
