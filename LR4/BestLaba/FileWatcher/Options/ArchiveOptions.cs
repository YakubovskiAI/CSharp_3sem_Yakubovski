using System;
using System.Text;
using System.IO.Compression;

namespace FileWatcher
{
    class ArchiveOptions
    {
        public ArchiveOptions() { }
        public bool NeedToArchive { get; set; }
        public bool NeedToCompress { get; set; }
        public CompressionLevel Level { get; set; }

    }
}
