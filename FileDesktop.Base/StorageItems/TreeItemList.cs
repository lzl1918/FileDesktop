using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.StorageItems
{
    public class TreeItemList<TData, TItem> : SortedList<string, TItem> where TItem : ITreeItem<TData, TItem>
    {

    }
}
