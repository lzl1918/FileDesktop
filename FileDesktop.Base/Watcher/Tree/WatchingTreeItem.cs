using Base.StorageItems;

namespace Base.Watcher.Tree
{
    public sealed class WatchingTreeItem : ITreeItem<ItemWatchingOptions, WatchingTreeItem>
    {
        public string Name { get; }

        public WatchingTreeItem Parent { get; }

        public TreeItemList<ItemWatchingOptions, WatchingTreeItem> Children { get; }

        public ItemType Type { get; }

        public ItemWatchingOptions Data { get; set; }


        internal WatchingTreeItem(string name, WatchingTreeItem parent, ItemType type)
        {
            Name = name;
            Parent = parent;
            Type = type;
            Children = new TreeItemList<ItemWatchingOptions, WatchingTreeItem>();
        }
    }
}
