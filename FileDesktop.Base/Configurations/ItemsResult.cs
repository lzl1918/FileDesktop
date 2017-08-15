using Base.StorageItems;
using Base.Watcher;
using Base.Watcher.Tree;
using System;
using System.Collections.Generic;
using System.IO;

namespace Base.Configurations
{
    public sealed class ItemsResult
    {
        public WatchingTree WatchingTree { get; }
        public IReadOnlyList<FileInfo> Files { get; }
        public IReadOnlyList<DirectoryInfo> Directories { get; }

        internal ItemsResult(IReadOnlyList<FileInfo> files, IReadOnlyList<DirectoryInfo> directories, WatchingTree watchingTree)
        {
            if (files == null)
                throw new ArgumentNullException(nameof(files));
            if (directories == null)
                throw new ArgumentNullException(nameof(directories));
            if (watchingTree == null)
                throw new ArgumentNullException(nameof(watchingTree));

            Files = files;
            Directories = directories;
            WatchingTree = watchingTree;
        }
    }
}
