using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Ryujinx.Ui.Common.Helper;
using System.Linq;
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
                     var storage = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions());

                     if (storage.Count > 0)
                     {
                         var folder = storage.First();
                         if (folder.TryGetUri(out var uri))
                         {
                             path = uri.LocalPath;
                         }
                     }
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