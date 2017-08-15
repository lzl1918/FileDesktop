using Base.Configurations;
using Base.StorageItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Base.Watcher
{
    public sealed class Watcher : IDisposable
    {
        public string WatchingDirectory { get; private set; }

        public List<string> ExpectingFiles { get; }
        public List<string> ExpectingDirectories { get; }
        public List<string> WatchedFiles { get; }
        public List<string> WatchedDirectories { get; }
        public List<DirectoryScanOptions> ScanOptions { get; }

        public event EventHandler<WatchingItemEventArgs> ItemAdded;
        public event EventHandler<WatchingItemEventArgs> ItemRemoved;
        public event EventHandler<WatcherFailedEventArgs> WatchingDirectoryRemoved;

        private FileSystemWatcher fileWatcher;
        private FileSystemWatcher directoryWatcher;
        private AutoResetEvent mutex;

        internal Watcher(string watchingDirectory, WatcherCreationOptions creationOptions)
        {
            WatchingDirectory = watchingDirectory;
            ExpectingDirectories = creationOptions.ExpectingDirectories;
            WatchedDirectories = creationOptions.WatchedDirectories;
            ExpectingFiles = creationOptions.ExpectingFiles;
            WatchedFiles = creationOptions.WatchedFiles;
            ScanOptions = creationOptions.ScanOptions;
        }

        public void BeginWatch()
        {
            mutex = new AutoResetEvent(true);
            InitWatcher();
        }
        private void InitWatcher()
        {
            fileWatcher = new FileSystemWatcher(WatchingDirectory);
            fileWatcher.BeginInit();
            fileWatcher.Error += OnFileWatcherError;
            fileWatcher.Created += OnFileWatcherCreated;
            fileWatcher.Deleted += OnFileWatcherDeleted;
            fileWatcher.Renamed += OnFileWatcherRenamed;
            fileWatcher.EnableRaisingEvents = true;
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.FileName;
            fileWatcher.EndInit();

            directoryWatcher = new FileSystemWatcher(WatchingDirectory);
            directoryWatcher.BeginInit();
            directoryWatcher.Created += OnDirectoryWatcherCreated;
            directoryWatcher.Deleted += OnDirectoryWatcherDeleted;
            directoryWatcher.Renamed += OnDirectoryWatcherRenamed;
            directoryWatcher.EnableRaisingEvents = true;
            directoryWatcher.IncludeSubdirectories = true;
            directoryWatcher.NotifyFilter = NotifyFilters.DirectoryName;
            directoryWatcher.EndInit();
        }

        private void OnFileWatcherRenamed(object sender, RenamedEventArgs e)
        {
            mutex.WaitOne();
            string removedPath = e.OldFullPath;
            string removedName = System.IO.Path.GetFileName(removedPath);
            string addedPath = e.FullPath;
            string addedName = System.IO.Path.GetFileName(addedPath);
            int index = WatchedFiles.IndexOf(removedPath);
            if (index >= 0)
            {
                WatchedFiles.RemoveAt(index);
                ExpectingFiles.Add(removedPath);
                ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.File, removedPath, removedName));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(removedName, removedPath, ItemType.File))
                    {
                        ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.File, removedPath, removedName));
                        break;
                    }
                }
            }
            index = ExpectingFiles.IndexOf(addedPath);
            if (index >= 0)
            {
                ExpectingFiles.RemoveAt(index);
                WatchedFiles.Add(addedPath);
                ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.File, addedPath, addedName));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(addedName, addedPath, ItemType.File))
                    {
                        ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.File, addedPath, addedName));
                        break;
                    }
                }
            }
            mutex.Set();
        }
        private void OnFileWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            mutex.WaitOne();
            string path = e.FullPath;
            string name = System.IO.Path.GetFileName(path);
            int index = WatchedFiles.IndexOf(path);
            if (index >= 0)
            {
                WatchedFiles.RemoveAt(index);
                ExpectingFiles.Add(path);
                ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.File, path, name));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(name, path, ItemType.File))
                    {
                        ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.File, path, name));
                        break;
                    }
                }
            }
            mutex.Set();
        }
        private void OnFileWatcherCreated(object sender, FileSystemEventArgs e)
        {
            mutex.WaitOne();
            string path = e.FullPath;
            string name = System.IO.Path.GetFileName(path);
            int index = ExpectingFiles.IndexOf(path);
            if (index >= 0)
            {
                ExpectingFiles.RemoveAt(index);
                WatchedFiles.Add(path);
                ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.File, path, name));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(name, path, ItemType.File))
                    {
                        ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.File, path, name));
                        break;
                    }
                }
            }
            mutex.Set();
        }
        private void OnFileWatcherError(object sender, ErrorEventArgs e)
        {
            mutex.WaitOne();
            Exception ex = e.GetException();
            WatcherFailedEventArgs failedArg = new WatcherFailedEventArgs();
            WatchingDirectoryRemoved?.Invoke(this, failedArg);
            mutex.Set();
            if (failedArg.Action == WatcherFailedAction.Dispose)
            {
                Dispose();
            }
            else
            {
                if (fileWatcher != null)
                    fileWatcher.Dispose();
                if (directoryWatcher != null)
                    directoryWatcher.Dispose();
                WatchingDirectory = failedArg.DirectoryIfResume;
                InitWatcher();
            }
        }

        private void OnDirectoryWatcherRenamed(object sender, RenamedEventArgs e)
        {
            mutex.WaitOne();
            string removedPath = e.OldFullPath;
            string removedName = System.IO.Path.GetFileName(removedPath);
            string addedPath = e.FullPath;
            string addedName = System.IO.Path.GetFileName(addedPath);
            int index = WatchedFiles.IndexOf(removedPath);
            if (index >= 0)
            {
                WatchedDirectories.RemoveAt(index);
                ExpectingDirectories.Add(removedPath);
                ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, removedPath, removedName));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(removedName, removedPath, ItemType.Directory))
                    {
                        ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, removedPath, removedName));
                        break;
                    }
                }
            }
            index = ExpectingDirectories.IndexOf(addedPath);
            if (index >= 0)
            {
                ExpectingDirectories.RemoveAt(index);
                WatchedDirectories.Add(addedPath);
                ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, addedPath, addedName));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(addedName, addedPath, ItemType.Directory))
                    {
                        ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, addedPath, addedName));
                        break;
                    }
                }
            }
            mutex.Set();
        }
        private void OnDirectoryWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            mutex.WaitOne();
            string path = e.FullPath;
            string name = System.IO.Path.GetFileName(path);
            int index = WatchedDirectories.IndexOf(path);
            if (index >= 0)
            {
                WatchedDirectories.RemoveAt(index);
                ExpectingDirectories.Add(path);
                ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, path, name));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(name, path, ItemType.Directory))
                    {
                        ItemRemoved?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, path, name));
                        break;
                    }
                }
            }
            mutex.Set();
        }
        private void OnDirectoryWatcherCreated(object sender, FileSystemEventArgs e)
        {
            mutex.WaitOne();
            string path = e.FullPath;
            string name = System.IO.Path.GetFileName(path);
            int index = ExpectingDirectories.IndexOf(path);
            if (index >= 0)
            {
                ExpectingDirectories.RemoveAt(index);
                WatchedDirectories.Add(path);
                ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, path, name));
            }
            else
            {
                foreach (DirectoryScanOptions scanOptions in ScanOptions)
                {
                    if (scanOptions.IsIncluded(name, path, ItemType.Directory))
                    {
                        ItemAdded?.Invoke(this, new WatchingItemEventArgs(ItemType.Directory, path, name));
                        break;
                    }
                }
            }
            mutex.Set();
        }


        #region IDisposable Support
        private bool disposedValue = false;
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (fileWatcher != null)
                    fileWatcher.Dispose();
                if (directoryWatcher != null)
                    directoryWatcher.Dispose();
                if (mutex != null)
                    mutex.Dispose();
                disposedValue = true;
            }
        }
        ~Watcher()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
