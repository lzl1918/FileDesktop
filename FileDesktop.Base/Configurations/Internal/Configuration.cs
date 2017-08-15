using Base.StorageItems;
using Base.Watcher;
using Base.Watcher.Tree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Configurations
{
    internal sealed class Configuration : IConfiguartion
    {
        public ScanFilterOptions GloabalExclude { get; }

        public ItemIncludeOptions IncludeItems { get; }

        public List<DirectoryScanOptions> ScanDirectories { get; }

        internal Configuration(ScanFilterOptions globalExclude, ItemIncludeOptions includeItems, List<DirectoryScanOptions> scanDirectories)
        {
            if (globalExclude == null)
                throw new ArgumentNullException(nameof(globalExclude));
            if (includeItems == null)
                throw new ArgumentNullException(nameof(includeItems));
            if (scanDirectories == null)
                throw new ArgumentNullException(nameof(scanDirectories));

            GloabalExclude = globalExclude;
            IncludeItems = includeItems;
            ScanDirectories = scanDirectories;
        }

        public ItemsResult EnumerateItems()
        {
            WatchingTree watchingTree = new WatchingTree();
            List<DirectoryInfo> directories = new List<DirectoryInfo>();
            List<FileInfo> files = new List<FileInfo>();
            EnumerateIncludeItems(directories, files, watchingTree);
            EnumerateScanDirectories(directories, files, watchingTree);
            return new ItemsResult(files, directories, watchingTree);
        }
        private void EnumerateScanDirectory(DirectoryScanOptions scanOptions, List<DirectoryInfo> directories, List<FileInfo> files, WatchingTree watchingTree)
        {
            WatchingTreeItem item = watchingTree.ItemByPath(scanOptions.DirectoryPath, ItemType.Directory);
            item.Data = new ItemWatchingOptions()
            {
                ScanOptions = scanOptions,
                WatchingStatus = ItemWatchingStatus.ScanNeeded
            };

            if (!Directory.Exists(scanOptions.DirectoryPath))
                return;
            Queue<DirectoryInfo> dirque = new Queue<DirectoryInfo>();
            DirectoryInfo dir = new DirectoryInfo(scanOptions.DirectoryPath);
            dirque.Enqueue(dir);
            dirque.Enqueue(null);
            int currentDepth = 1;
            int maxdepth = scanOptions.ScanDepth;
            DirectoryInfo current;
            ScanFilterOptions excludeFilters = scanOptions.Excludes;
            ScanFilterOptions includeFilters = scanOptions.Includes;

            while (true)
            {
                current = dirque.Dequeue();
                if (current == null)
                {
                    if (dirque.Count <= 0)
                        break;

                    currentDepth++;
                    if (currentDepth > maxdepth)
                    {
                        foreach (DirectoryInfo directory in dirque)
                        {
                            if (!excludeFilters.IsMatch(directory) &&
                                includeFilters.IsMatch(directory))
                                directories.Add(directory);
                        }
                        break;
                    }
                    else
                    {
                        dirque.Enqueue(null);
                        continue;
                    }
                }

                IEnumerable<DirectoryInfo> enumeratedDirectories = current.EnumerateDirectories();
                if (!includeFilters.IsDefault)
                {
                    foreach (DirectoryInfo directory in enumeratedDirectories)
                    {
                        if (includeFilters.IsMatch(directory))
                            directories.Add(directory);
                        else if (!excludeFilters.IsMatch(directory))
                            dirque.Enqueue(directory);
                    }
                }
                else
                {
                    foreach (DirectoryInfo directory in enumeratedDirectories)
                    {
                        if (!excludeFilters.IsMatch(directory))
                            dirque.Enqueue(directory);
                    }
                }


                IEnumerable<FileInfo> enumeratedFiles = current.EnumerateFiles();
                if (!includeFilters.IsDefault)
                {
                    foreach (FileInfo file in enumeratedFiles)
                    {
                        if (includeFilters.IsMatch(file))
                            files.Add(file);
                    }
                }
                else
                {
                    foreach (FileInfo file in enumeratedFiles)
                    {
                        if (!excludeFilters.IsMatch(file) && includeFilters.IsMatch(file))
                            files.Add(file);
                    }
                }
            }
        }
        private void EnumerateScanDirectories(List<DirectoryInfo> directories, List<FileInfo> files, WatchingTree watchingTree)
        {
            foreach (DirectoryScanOptions scanOptions in ScanDirectories)
                EnumerateScanDirectory(scanOptions, directories, files, watchingTree);
        }
        private void EnumerateIncludeItems(List<DirectoryInfo> directories, List<FileInfo> files, WatchingTree watchingTree)
        {
            foreach (string path in IncludeItems.Files)
            {
                WatchingTreeItem item = watchingTree.ItemByPath(path, ItemType.File);
                item.Data = new ItemWatchingOptions();
                if (File.Exists(path))
                {
                    files.Add(new FileInfo(path));
                    item.Data.WatchingStatus = ItemWatchingStatus.Watched;
                }
                else
                {
                    item.Data.WatchingStatus = ItemWatchingStatus.Expected;
                }

            }
            foreach (string path in IncludeItems.Directories)
            {
                WatchingTreeItem item = watchingTree.ItemByPath(path, ItemType.Directory);
                item.Data = new ItemWatchingOptions();
                if (Directory.Exists(path))
                {
                    directories.Add(new DirectoryInfo(path));
                    item.Data.WatchingStatus = ItemWatchingStatus.Watched;
                }
                else
                {
                    item.Data.WatchingStatus = ItemWatchingStatus.Expected;
                }
            }
        }
    }
}
