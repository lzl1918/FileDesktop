namespace Base.StorageItems
{
    public interface ITree<TData, TItem> where TItem : ITreeItem<TData, TItem>
    {
        TItem Root { get; }

        /// <summary>
        /// retrive item or create item by path
        /// </summary>
        /// <param name="path">path of the item</param>
        /// <param name="creationType">item type if creation needed</param>
        /// <returns></returns>
        TItem ItemByPath(string path, ItemType creationType);

        /// <summary>
        /// retrive item by a specific path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        TItem ItemByPath(string path);

        string PathOfItem(TItem item);

        void Remove(TItem item);
    }

}
