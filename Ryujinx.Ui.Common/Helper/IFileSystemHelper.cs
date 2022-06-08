using System.IO;
using System.Threading.Tasks;

namespace Ryujinx.Ui.Common.Helper
{
    public interface IFileSystemHelper
    {
        Stream GetContentStream(string uri);

        string[] GetFileEntries(string directory, string search);
        string[] GetDirectories(string directory);
        bool FileExist(string uri);
        bool DirectoryExist(string directory);
        long GetFileLength(string file);
        Task<string> OpenFolder(object parent);
    }
}