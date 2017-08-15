using Base.Configurations;
using Base.Watcher.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Base.Watcher
{
    internal sealed class WatcherCreationOptions
    {
        public List<string> WatchedDirectories { get; }
        public List<string> WatchedFiles { get; }
        public List<string> ExpectingDirectories { get; }
        public List<string> ExpectingFiles { get; }
        public List<DirectoryScanOptions> ScanOptions { get; }

        public WatcherCreationOptions(List<string> watchedDirectories, List<string> watchedFiles, List<string> expectingDirectories, List<string> expectingFiles, List<DirectoryScanOptions> scanOptions)
        {
            WatchedDirectories = watchedDirectories;
            WatchedFiles = watchedFiles;
            ExpectingDirectories = expectingDirectories;
            ExpectingFiles = expectingFiles;
            ScanOptions = scanOptions;
        }
    }
    public sealed class WatcherManager : IDisposable
    {
        public event EventHandler<WatchingItemEventArgs> ItemAdded;
        public event EventHandler<WatchingItemEventArgs> ItemRemoved;

        private List<Watcher> watchers;
        private WatchingTree tree;
        private WatcherManager(List<Watcher> watchers, WatchingTree tree)
        {
            this.watchers = watchers;
            this.tree = tree;
        }
        public void BeginWatch()
        {
            foreach (Watcher watcher in watchers)
            {
                watcher.ItemAdded += OnWatcherItemAdded;
                watcher.ItemRemoved += OnWatcherItemRemoved;
                watcher.WatchingDirectoryRemoved += OnWatcherFailed;
                watcher.BeginWatch();
            }
        }

        private void OnWatcherFailed(object sender, WatcherFailedEventArgs e)
        {
            Watcher watcher = sender as Watcher;
            if (watcher == null)
                return;

            e.Action = WatcherFailedAction.Dispose;
            watcher.ItemAdded -= OnWatcherItemAdded;
            watcher.ItemRemoved -= OnWatcherItemRemoved;
            watcher.WatchingDirectoryRemoved -= OnWatcherFailed;
            watchers.Remove(watcher);
        }

        private void OnWatcherItemRemoved(object sender, WatchingItemEventArgs e)
        {
            ItemRemoved?.Invoke(this, e);
        }

        private void OnWatcherItemAdded(object sender, WatchingItemEventArgs e)
        {
            ItemAdded?.Invoke(this, e);
        }

        public static WatcherManager CreateFromTree(WatchingTree tree)
        {
            if (tree == null)
                throw new ArgumentNullException(nameof(tree));
            List<WatchingTreeItem> watchPoints = RetriveWatchPoints(tree);
            List<Watcher> watchers = new List<Watcher>();
            string path;
            foreach (WatchingTreeItem watchPoint in watchPoints)
            {
                path = tree.PathOfItem(watchPoint);
                WatcherCreationOptions creationOptions = CollectNodes(watchPoint, path);
                watchers.Add(new Watcher(path, creationOptions));
            }
            return new WatcherManager(watchers, tree);
        }

        private static List<WatchingTreeItem> RetriveWatchPoints(WatchingTree tree)
        {
            List<WatchingTreeItem> result = new List<WatchingTreeItem>();
            Queue<WatchingTreeItem> itemsQue = new Queue<WatchingTreeItem>();
            WatchingTreeItem root = tree.Root;
            foreach (var pair in root.Children)
                itemsQue.Enqueue(pair.Value);

            WatchingTreeItem item;
            WatchingTreeItem child;
            bool childItems;
            while (itemsQue.Count > 0)
            {
                item = itemsQue.Dequeue();
                childItems = item.Children.Any(pair =>
                {
                    ItemWatchingOptions data = pair.Value.Data;
                    return data != null && data.WatchingStatus != ItemWatchingStatus.None;
                });
                if (childItems)
                {
                    result.Add(item);
                    continue;
                }
                foreach (var pair in item.Children)
                {
                    child = pair.Value;
                    itemsQue.Enqueue(child);
                }
            }
            return result;
        }
        private static WatcherCreationOptions CollectNodes(WatchingTreeItem current, string currentPath)
        {
            if (current == null)
                throw new ArgumentNullException(nameof(current));
            if (currentPath == null)
                throw new ArgumentNullException(nameof(currentPath));

            List<string> watchedDirectories = new List<string>();
            List<string> watchedFiles = new List<string>();
            List<string> expectingDirectories = new List<string>();
            List<string> expectingFiles = new List<string>();
            List<DirectoryScanOptions> scanOptions = new List<DirectoryScanOptions>();
            StringBuilder pathBuilder = new StringBuilder(256);
            pathBuilder.Append(currentPath);
            Stack<IEnumerator<KeyValuePair<string, WatchingTreeItem>>> enumeratorStack = new Stack<IEnumerator<KeyValuePair<string, WatchingTreeItem>>>();
            Stack<WatchingTreeItem> parentStack = new Stack<WatchingTreeItem>();
            parentStack.Push(current);
            enumeratorStack.Push(current.Children.GetEnumerator());
            IEnumerator<KeyValuePair<string, WatchingTreeItem>> enumerator;
            WatchingTreeItem parent;
            WatchingTreeItem cur;
            int shrink;
            while (enumeratorStack.Count > 0)
            {
                enumerator = enumeratorStack.Peek();
                parent = parentStack.Peek();
                if (enumerator.MoveNext())
                {
                    cur = enumerator.Current.Value;
                    enumeratorStack.Push(cur.Children.GetEnumerator());
                    parentStack.Push(cur);
                    if (pathBuilder.Length > 0)
                        pathBuilder.Append(Path.PathHelper.SEPERATOR);
                    pathBuilder.Append(cur.Name);
                }
                else
                {
                    if (parent.Data != null)
                    {
                        switch (parent.Data.WatchingStatus)
                        {
                            case ItemWatchingStatus.Watched:
                                if (parent.Type == StorageItems.ItemType.File)
                                    watchedFiles.Add(pathBuilder.ToString());
                                else
                                    watchedDirectories.Add(pathBuilder.ToString());
                                break;
                            case ItemWatchingStatus.Expected:
                                if (parent.Type == StorageItems.ItemType.File)
                                    expectingFiles.Add(pathBuilder.ToString());
                                else
                                    expectingDirectories.Add(pathBuilder.ToString());
                                break;
                            case ItemWatchingStatus.ScanNeeded:
                                scanOptions.Add(parent.Data.ScanOptions);
                                break;
                            case ItemWatchingStatus.None:
                            default:
                                break;
                        }
                    }
                    shrink = parent.Name.Length + 1;
                    if (shrink > pathBuilder.Length)
                        pathBuilder.Clear();
                    else
                        pathBuilder.Remove(pathBuilder.Length - shrink, shrink);
                    enumeratorStack.Pop();
                    parentStack.Pop();
                }
            }
            return new WatcherCreationOptions(watchedDirectories, watchedFiles, expectingDirectories, expectingFiles, scanOptions);
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

                foreach (Watcher watcher in watchers)
                {
                    watcher.ItemAdded -= OnWatcherItemAdded;
                    watcher.ItemRemoved -= OnWatcherItemRemoved;
                    watcher.WatchingDirectoryRemoved -= OnWatcherFailed;
                    watcher.Dispose();
                }
                disposedValue = true;
            }
        }
        ~WatcherManager()
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
