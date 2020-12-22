using System;
using System.IO;
using System.IO.Compression;

namespace FileWatcher
{
    class ArchiveManager
    {
        public void Compress(string sourceFile, string compressedFile, CompressionLevel level)
        {
            using (FileStream sourceStream = new FileStream(sourceFile, FileMode.OpenOrCreate))
            {
                using (FileStream targetStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
                {
                    using (GZipStream compressionStream = new GZipStream(targetStream, level))
                    {
                        sourceStream.CopyTo(compressionStream);
                    }
                }
            }
        }

        public void Decompress(string compressedFile, string targetFile)
        {
            using (FileStream sourceStream = new FileStream(compressedFile, FileMode.OpenOrCreate))
            {
                using (FileStream targetStream = new FileStream(targetFile, FileMode.OpenOrCreate))
                {
                    using (GZipStream decompressionStream = new GZipStream(sourceStream, CompressionMode.Decompress))
                    {
                        decompressionStream.CopyTo(targetStream);
                    }
                }
            }
        }

        public void AddToArchive(string filePath, string fPath)
        {
            var archivePath = Path.Combine(fPath, "archive.zip");

            using (ZipArchive zipArchive = ZipFile.Open(archivePath, ZipArchiveMode.Update))
            {
                zipArchive.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
            }
        }
    }
}
