using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcherConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "D:\\SourceDirectory";
            string filePath = "D:\\SourceDirectory\\file.txt";
            string fileName = Path.GetFileName(filePath);
            var dt = DateTime.Now;

            string subPath =
               $"{dt.ToString("yyyy", DateTimeFormatInfo.InvariantInfo)}\\" +
               $"{dt.ToString("MM", DateTimeFormatInfo.InvariantInfo)}\\" +
               $"{dt.ToString("dd", DateTimeFormatInfo.InvariantInfo)}";

            string newPath = $"D:\\SourceDirectory\\" +
               $"{dt.ToString("yyyy", DateTimeFormatInfo.InvariantInfo)}\\" +
               $"{dt.ToString("MM", DateTimeFormatInfo.InvariantInfo)}\\" +
               $"{dt.ToString("dd", DateTimeFormatInfo.InvariantInfo)}\\" +
               $"{Path.GetFileNameWithoutExtension(fileName)}_" +
               $"{dt.ToString(@"yyyy_MM_dd_HH_mm_ss", DateTimeFormatInfo.InvariantInfo)}" +
               $"{Path.GetExtension(fileName)}";

            DirectoryInfo dirInfo = new DirectoryInfo(path);

            if(!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            dirInfo.CreateSubdirectory(subPath);
            File.Move(filePath, newPath);
            EncryptFile(newPath, newPath);

            var compressedPath = Path.ChangeExtension(newPath, "gz");
            var newCompressedPath = Path.Combine("D:\\TargetDirectory", Path.GetFileName(compressedPath));
            var decompressedPath = Path.ChangeExtension(newCompressedPath, "txt");
            Compress(newPath, compressedPath);

            File.Move(compressedPath, newCompressedPath);
            Decompress(newCompressedPath, decompressedPath);
            DecryptFile(decompressedPath, decompressedPath);
            AddToArchive(decompressedPath);

            File.Delete(newPath);
            File.Delete(newCompressedPath);
            File.Delete(decompressedPath);
        }

        private static void EncryptFile(string inputFile, string outputFile)
        {
                string password = "Postavte_norm_ocenku"; 
                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                byte[] arr = File.ReadAllBytes(inputFile);

                string cryptFile = outputFile;
                using (FileStream fsCrypt = new FileStream(cryptFile, FileMode.Create))
                {
                    RijndaelManaged RMCrypto = new RijndaelManaged();

                    using (CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateEncryptor(key, key),
                        CryptoStreamMode.Write))
                    {
                            foreach (byte bt in arr)
                            {
                                cs.WriteByte(bt);
                            }
                    }
                }
        }

        private static void DecryptFile(string inputFile, string outputFile)
        {
            {
                string password = "Postavte_norm_ocenku"; 

                UnicodeEncoding UE = new UnicodeEncoding();
                byte[] key = UE.GetBytes(password);
                var bt = new List<byte>();

                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    RijndaelManaged RMCrypto = new RijndaelManaged();

                    using (CryptoStream cs = new CryptoStream(fsCrypt, RMCrypto.CreateDecryptor(key, key),
                        CryptoStreamMode.Read))
                    {
                        int data;
                        while ((data = cs.ReadByte()) != -1)
                        {
                            bt.Add((byte)data);
                        }
                    }
                }
                using (FileStream fsOut = new FileStream(outputFile, FileMode.Create))
                {
                    foreach (var b in bt)
                    {
                        fsOut.WriteByte(b);
                    }
                }
            }
        }

        private static void Compress(string sourceFile, string compressedFile)
        {
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                using (FileStream targetStream = File.Create(compressedFile))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, CompressionMode.Compress))
                    {
                        sourceStream.CopyTo(compressionStream); 
                    }
                }
            }
        }

        private static void Decompress(string compressedFile, string targetFile)
        {
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                using (FileStream targetStream = File.Create(targetFile))
                {
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                        Console.WriteLine("Восстановлен файл: {0}", targetFile);
                    }
                }
            }
        }

        private static void AddToArchive(string filePath)
        {
            string archivePath = "D:\\TargetDirectory\\archive.zip";

            using (ZipArchive zipArchive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
            {
                zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
            }
        }
    }
}
