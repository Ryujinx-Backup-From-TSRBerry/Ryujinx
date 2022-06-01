using System.IO;

namespace Ryujinx.Ui.Common.Helper
{
    public interface IFileSystemHelper
    {
        Stream GetContentStream(string uri);

        string[] GetFileEntries(string directory, string search);
        string[] GetDirectories(string directory);
        bool FileExist(string uri);
        bool DirectoryExist(string directory);
    }
}