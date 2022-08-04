using Android.Content;
using Android.Provider;
using AndroidX.DocumentFile.Provider;
using Ryujinx.Ui.Common.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Ryujinx.Rsc.Mobile.Helper
{
    public class AndroidFileSystemHelper : IFileSystemHelper, IDisposable
    {
        public const int FileRequestCode = 100;

        private readonly MainActivity _activity;
        private readonly IFileSystemHelper _defaultFileSystemHelper;
        private readonly ManualResetEvent _resetEvent;
        private FileSystemResultEventArgs _currentResult;

        public AndroidFileSystemHelper(MainActivity activiy)
        {
            _defaultFileSystemHelper = new FileSystemHelper();
            _activity = activiy;
            _activity.FileSystemResult += Activity_FileSystemResult;
            _resetEvent = new ManualResetEvent(false);
        }

        private void Activity_FileSystemResult(object sender, FileSystemResultEventArgs e)
        {
            _currentResult = e;
            _resetEvent.Set();
        }

        public bool DirectoryExist(string directory)
        {
            if (!RequiresScopedStorageAccess(directory))
            {
                return _defaultFileSystemHelper.DirectoryExist(directory);
            }
            else
            {
                return EnsureAccessPermitted(directory);
            }
        }

        public bool FileExist(string uri)
        {
            if (!RequiresScopedStorageAccess(uri))
            {
                return _defaultFileSystemHelper.FileExist(uri);
            }
            else
            {
                try
                {
                    var parsedUri = Android.Net.Uri.Parse(uri);
                    var file = DocumentFile.FromSingleUri(_activity, parsedUri);

                    return file != null && file.Exists();
                }
                catch (Exception _)
                {
                    return false;
                }
            }
        }

        public Stream GetContentStream(string uri)
        {
            if (!RequiresScopedStorageAccess(uri))
            {
                return _defaultFileSystemHelper.GetContentStream(uri);
            }
            else
            {
                var parsedUri = Android.Net.Uri.Parse(uri);
                var resolver = _activity.ContentResolver;
                return resolver.OpenInputStream(parsedUri);
            }
        }

        public string[] GetDirectories(string directory)
        {
            if (!RequiresScopedStorageAccess(directory))
            {
                return _defaultFileSystemHelper.GetDirectories(directory);
            }
            else
            {
                if (EnsureAccessPermitted(directory))
                {
                    var uri = Android.Net.Uri.Parse(directory);
                    var documentFileTree = DocumentFile.FromTreeUri(_activity, uri);
                    var directories = new List<string>();

                    var children = documentFileTree.ListFiles();
                    foreach (var document in children)
                    {
                        if (document.IsDirectory)
                        {
                            directories.Add(document.Uri.ToString());
                        }
                    }

                    return directories.ToArray();
                }

                return Array.Empty<string>();
            }
        }

        public string[] GetFileEntries(string directory, string search)
        {
            if (!RequiresScopedStorageAccess(directory))
            {
                return _defaultFileSystemHelper.GetFileEntries(directory, search);
            }
            else
            {
                if (EnsureAccessPermitted(directory))
                {
                    var uri = Android.Net.Uri.Parse(directory);
                    var documentFileTree = DocumentFile.FromTreeUri(_activity, uri);
                    var files = new List<string>();

                    var children = documentFileTree.ListFiles();
                    foreach (var document in children)
                    {
                        if (document.IsFile && FileSystemName.MatchesSimpleExpression(search, document.Uri.Path))
                        {
                            files.Add(document.Uri.ToString());
                        }
                    }

                    return files.ToArray();
                }

                return Array.Empty<string>();
            }
        }

        private bool RequiresScopedStorageAccess(string uri)
        {
            return !(Android.OS.Build.VERSION.SdkInt <= Android.OS.BuildVersionCodes.Q ||
                    uri.StartsWith(_activity.GetExternalFilesDir(null).AbsolutePath));
        }

        private bool EnsureAccessPermitted(string directory)
        {
            var uri = Android.Net.Uri.Parse(directory);
            try
            {
                var documentFileTree = DocumentFile.FromTreeUri(_activity, uri);
                documentFileTree.Dispose();
                _activity.ContentResolver?.TakePersistableUriPermission(uri, ActivityFlags.GrantReadUriPermission);
            }
            catch (Exception _)
            {
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _activity.FileSystemResult -= Activity_FileSystemResult;
            _resetEvent.Set();
            _resetEvent.Dispose();
        }

        public async Task<string> OpenFolder(object parent)
        {
            var intent = new Intent(Intent.ActionOpenDocumentTree);

            _activity.RunOnUiThread(() =>
            {
                _activity.StartActivityForResult(intent, FileRequestCode);
            });

            _resetEvent.Reset();
            _resetEvent.WaitOne();

            var result = _currentResult;
            _currentResult = null;

            if (result != null)
            {
                if (result.ResultCode == Android.App.Result.Ok)
                {
                    try
                    {
                        var flags = result.Data.Flags & (ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                        _activity.ContentResolver?.TakePersistableUriPermission(result.Data.Data, flags);
                    }
                    catch (Exception ex)
                    {

                    }
                    return result.Data.Data.ToString();
                }
            }

            return string.Empty;
        }

        public long GetFileLength(string file)
        {
            var uri = Android.Net.Uri.Parse(file);
            using var documentFile = DocumentFile.FromSingleUri(_activity, uri);
            return documentFile.Length();
        }

        public void DeleteFile(string file)
        {
            var uri = Android.Net.Uri.Parse(file);
            using var documentFile = DocumentFile.FromSingleUri(_activity, uri);
            documentFile.Delete();
        }
    }
}