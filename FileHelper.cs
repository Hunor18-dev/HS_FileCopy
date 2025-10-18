using System;
using System.IO;
using System.Security.Cryptography;

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

        public byte[] GetChecksum(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    //BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return md5.ComputeHash(stream);
                }
            }
        }

    }
}
