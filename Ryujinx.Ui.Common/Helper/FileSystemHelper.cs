using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ryujinx.Ui.Common.Helper
{
    public class FileSystemHelper : IFileSystemHelper
    {
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
    }
}