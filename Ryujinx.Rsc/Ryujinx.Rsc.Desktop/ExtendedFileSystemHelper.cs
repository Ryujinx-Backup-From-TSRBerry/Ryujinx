using Avalonia.Controls;
using Avalonia.Threading;
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
                string path = string.Empty;
                await Dispatcher.UIThread.InvokeAsync(async () =>
                 {
                     path = await new OpenFolderDialog().ShowAsync(window);
                 });

                return path;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}