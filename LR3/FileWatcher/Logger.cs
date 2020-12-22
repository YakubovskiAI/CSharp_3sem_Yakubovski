using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;

namespace FileWatcher
{
    class Logger
    {
        private FileSystemWatcher watcher;
        private readonly StringBuilder messages = new StringBuilder();
        private readonly Options options;
        private readonly object obj = new object();
        bool enabled = true;
        private EncryptManager encryptManager = new EncryptManager();
        private ArchiveManager archiveManager = new ArchiveManager();
        public Logger(Options options)
        {
            this.options = options;

            if (!Directory.Exists(this.options.SourcePath))
            {
                Directory.CreateDirectory(this.options.SourcePath);
            }

            if (!Directory.Exists(this.options.TargetPath))
            {
                Directory.CreateDirectory(this.options.TargetPath);
            }

            watcher = new FileSystemWatcher(this.options.SourcePath);
            watcher.Filter = "*.txt";
            watcher.Deleted += OnDeleted;
            watcher.Created += OnCreated;
            watcher.Changed += OnChanged;
            watcher.Renamed += OnRenamed;
            watcher.Created += FileTransfer;
        }

        public void Start()
        {
            WriteToFile($"Service was started at {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
            watcher.EnableRaisingEvents = true;
            while (enabled)
            {
                Thread.Sleep(1000);
            }
        }

        public void Stop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
            messages.Clear();
            WriteToFile($"Service was stopped at {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n");
        }

        private void FileTransfer(object sender, FileSystemEventArgs e)
        {
            if (!Directory.Exists(this.options.SourcePath))
            {
                Directory.CreateDirectory(this.options.SourcePath);
                watcher = new FileSystemWatcher(this.options.SourcePath);
                watcher.Filter = "*.txt";
                watcher.Deleted += OnDeleted;
                watcher.Created += OnCreated;
                watcher.Changed += OnChanged;
                watcher.Renamed += OnRenamed;
                watcher.Created += FileTransfer;
            }

            if (!Directory.Exists(this.options.TargetPath))
            {
                Directory.CreateDirectory(this.options.TargetPath);
            }

            if (messages.Length > 0)
            {
                WriteToFile(messages.ToString());
                messages.Clear();
            }

            lock (obj)
            {
                try
                {
                    var dirInfo = new DirectoryInfo(this.options.SourcePath);
                    var filePath = Path.Combine(this.options.SourcePath, e.Name);
                    var fileName = e.Name;
                    var dt = DateTime.Now;
                    var subPath = Path.Combine(dt.ToString("yyyy", DateTimeFormatInfo.InvariantInfo), dt.ToString("MM", DateTimeFormatInfo.InvariantInfo),
                                  dt.ToString("dd", DateTimeFormatInfo.InvariantInfo));
                    var newPathOne = Path.Combine(this.options.SourcePath, subPath, Path.GetFileNameWithoutExtension(fileName) + "_" +
                        dt.ToString(@"yyyy_MM_dd_HH_mm_ss", DateTimeFormatInfo.InvariantInfo) + Path.GetExtension(fileName));
                    var newPath = Path.Combine(this.options.SourcePath, subPath, Path.GetFileNameWithoutExtension(fileName) + "_" +
                        dt.ToString(@"yyyy_MM_dd_HH_mm_ss", DateTimeFormatInfo.InvariantInfo)+ "_1" + Path.GetExtension(fileName));
                    var newPathTarget = Path.Combine(this.options.TargetPath, Path.GetFileNameWithoutExtension(fileName) + "_" +
                       dt.ToString(@"yyyy_MM_dd_HH_mm_ss", DateTimeFormatInfo.InvariantInfo) + Path.GetExtension(fileName));
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }
                    dirInfo.CreateSubdirectory(subPath);

                    if (options.NeedToEncrypt)
                    {
                        File.Move(filePath, newPathOne);                        
                        encryptManager.EncryptFile(newPathOne, newPath);
                    }
                    else
                    {
                        File.Move(filePath, newPath);
                    }
                    
                    if(options.ArchiveOptions.NeedToCompress)
                    {
                        var compressedPath = Path.ChangeExtension(newPath, "gz");
                        var newCompressedPath = Path.Combine(options.TargetPath, Path.GetFileName(compressedPath));
                        var decompressedPath = Path.ChangeExtension(newCompressedPath, "txt");
                        archiveManager.Compress(newPath, compressedPath, options.ArchiveOptions.Level);
                        File.Delete(newPath);
                        File.Move(compressedPath, newCompressedPath);
                        archiveManager.Decompress(newCompressedPath, decompressedPath);
                        File.Delete(newCompressedPath);
                    }
                    else
                    {
                        File.Move(newPath, Path.Combine(options.TargetPath, Path.GetFileName(newPath)));               
                    }

                    var decryptPath = Path.Combine(options.TargetPath, Path.GetFileName(newPathOne));

                    if (options.NeedToEncrypt)
                    {
                        encryptManager.DecryptFile(Path.Combine(options.TargetPath, Path.GetFileName(newPath)), decryptPath);
                        File.Delete(Path.Combine(options.TargetPath, Path.GetFileName(newPath)));
                    }
                    else
                    {                       
                        File.Move(Path.Combine(options.TargetPath, Path.GetFileName(newPathOne)), decryptPath);
                    }

                    if(options.ArchiveOptions.NeedToArchive)
                    {
                        archiveManager.AddToArchive(decryptPath, options.TargetPath);
                        File.Delete(decryptPath);
                    }                                
                }
                catch (Exception ex)
                {
                    using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Exceptions.txt"), true))
                    {
                        sw.WriteLine($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} Exception: {ex.Message}");
                    }
                }
            }
        }       

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            string fileEvent = "created";
            AddToMessages(filePath, fileEvent);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            string filePath = e.OldFullPath;
            string fileEvent = "renamed to " + e.FullPath;
            AddToMessages(filePath, fileEvent);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            string fileEvent = "changed";
            AddToMessages(filePath, fileEvent);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            string fileEvent = "deleted";
            AddToMessages(filePath, fileEvent);
        }

        void AddToMessages(string filePath, string fileEvent)
        {
            messages.Append($"{DateTime.Now:dd/MM/yyyy HH:mm:ss} file {filePath} was {fileEvent}\n");
        }

        public void WriteToFile(string message)
        {
            if (!Directory.Exists(options.SourcePath))
            {
                Directory.CreateDirectory(options.SourcePath);

                watcher = new FileSystemWatcher(options.SourcePath);
                watcher.Deleted += OnDeleted;
                watcher.Created += OnCreated;
                watcher.Changed += OnChanged;
                watcher.Renamed += OnRenamed;

                watcher.EnableRaisingEvents = true;
            }

            if (!Directory.Exists(options.TargetPath))
            {
                Directory.CreateDirectory(options.TargetPath);
            }

            lock (obj)
            {
                using (StreamWriter sw = new StreamWriter(options.LogFilePath, true))
                {
                    sw.Write(message);
                }
            }
        }
    }
} 

