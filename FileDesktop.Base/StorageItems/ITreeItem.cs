namespace Base.StorageItems
{
    public interface ITreeItem<TData, TItem> where TItem : ITreeItem<TData, TItem>
    {
        string Name { get; }
        TItem Parent { get; }
        TreeItemList<TData, TItem> Children { get; }
        ItemType Type { get; }
        TData Data { get; set; }
    }
}
