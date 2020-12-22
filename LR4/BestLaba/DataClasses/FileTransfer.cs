using System.IO;

namespace ServiceLib
{ 
    public class FileTransfer
    {
        readonly string ftpFolder;

        readonly string outputFolder;

        public FileTransfer(string outputFolder, string ftpFolder)
        {
            this.ftpFolder = ftpFolder;

            this.outputFolder = outputFolder;
        }

        public void SendFileToFtp(string fileName)
        {
            if (File.Exists(Path.Combine(ftpFolder, fileName)))
            {
                File.Delete(Path.Combine(ftpFolder, fileName));
            }

            File.Copy(Path.Combine(outputFolder, fileName), Path.Combine(ftpFolder, fileName));
        }
    }
}
