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

        public bool DirectoryExists(string dirPath)
        {
            return Directory.Exists(dirPath);
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
                    return md5.ComputeHash(stream);
                }
            }
        }

        public byte[] GetChecksum(byte[] byteStream, int bytesRead)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(byteStream, 0, bytesRead))
                {
                    
                    return md5.ComputeHash(stream);
                }
            }
        }

        public byte[] GetChecksum(FileStream fileStream)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(fileStream);
            }
        }

    }
}
