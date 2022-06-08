using Avalonia.Controls;
using Ryujinx.Ui.Common.Helper;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.Desktop
{
    public class ExtendedFileSystemHelper : FileSystemHelper
    {
        public override async Task<string> OpenFolder(object parent)
        {
            if (parent is Window window)
            {
                return await new OpenFolderDialog().ShowAsync(window);
            }
            else
            {
                return string.Empty;
            }
        }
    }
}