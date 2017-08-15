using Base.StorageItems;
using System;

namespace Base.Watcher
{
    public sealed class WatchingItemEventArgs : EventArgs
    {
        public ItemType Type { get; }
        public string Path { get; }
        public string Name { get; }
        internal WatchingItemEventArgs(ItemType type, string path, string name)
        {
            Type = type;
            Path = path;
            Name = name;
        }
    }

    public enum WatcherFailedAction
    {
        ResumeWithNewDirectory,
        Dispose
    }
    public sealed class WatcherFailedEventArgs : EventArgs
    {
        public WatcherFailedAction Action { get; set; }
        public string DirectoryIfResume { get; set; }

        internal WatcherFailedEventArgs()
        {
            Action = WatcherFailedAction.Dispose;
        }
        internal WatcherFailedEventArgs(string directory)
        {
            Action = WatcherFailedAction.ResumeWithNewDirectory;
            DirectoryIfResume = directory;
        }
    }
}
