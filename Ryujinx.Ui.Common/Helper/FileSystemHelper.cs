using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ryujinx.Ui.Common.Helper
{
    public class FileSystemHelper : IFileSystemHelper
    {
        public void DeleteFile(string file)
        {
            File.Delete(file);
        }

        public bool DirectoryExist(string directory)
        {
            return Directory.Exists(directory);
        }

        public bool FileExist(string uri)
        {
            return File.Exists(uri);
        }

        public Stream GetContentStream(string uri)
        {
            return new FileStream(uri, FileMode.Open, FileAccess.Read);
        }

        public string[] GetDirectories(string directory)
        {
            return Directory.GetDirectories(directory);
        }

        public string[] GetFileEntries(string directory, string search)
        {
            return Directory.GetFiles(directory, search);
        }

        public long GetFileLength(string file)
        {
            return new FileInfo(file).Length;
        }

        public virtual async Task<string> OpenFolder(object parent)
        {
            return string.Empty;
        }
    }
}