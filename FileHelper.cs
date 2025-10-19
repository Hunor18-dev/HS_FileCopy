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

        public byte[] GetHashSHA256(string filePath)
        {
            using (var sha256 = SHA256.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return sha256.ComputeHash(stream);
                }
            }
        }

        public byte[] GetHashMD5(byte[] byteStream, int bytesRead)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = new MemoryStream(byteStream, 0, bytesRead))
                {
                    
                    return md5.ComputeHash(stream);
                }
            }
        }
    }
}
