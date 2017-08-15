using Base.StorageItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Watcher.Tree
{
    public sealed class WatchingTree : ITree<ItemWatchingOptions, WatchingTreeItem>
    {
        public WatchingTreeItem Root { get; }

        public WatchingTreeItem ItemByPath(string path, ItemType creationType)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            while (path.Length > 0 && (path[path.Length - 1] == '/' || path[path.Length - 1] == '\\'))
                path = path.Substring(0, path.Length - 1);

            string[] nodes = path.Split(Path.PathHelper.SEPERATOR);
            string name;
            WatchingTreeItem current = Root;
            WatchingTreeItem next;
            int i = 0;
            for (; i < nodes.Length - 1; i++)
            {
                name = nodes[i];
                if (!current.Children.TryGetValue(name, out next))
                {
                    next = new WatchingTreeItem(name, current, ItemType.Directory);
                    current.Children.Add(name, next);
                }
                current = next;
            }
            name = nodes[i];
            if (!current.Children.TryGetValue(name, out next))
            {
                next = new WatchingTreeItem(name, current, creationType);
                current.Children.Add(name, next);
            }
            return next;
        }

        public WatchingTreeItem ItemByPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));

            while (path.Length > 0 && (path[path.Length - 1] == '/' || path[path.Length - 1] == '\\'))
                path = path.Substring(0, path.Length - 1);

            string[] nodes = path.Split(Path.PathHelper.SEPERATOR);
            string name;
            WatchingTreeItem current = Root;
            WatchingTreeItem next;
            int i = 0;
            for (; i < nodes.Length - 1; i++)
            {
                name = nodes[i];
                if (!current.Children.TryGetValue(name, out next))
                {
                    return null;
                }
                current = next;
            }
            name = nodes[i];
            if (!current.Children.TryGetValue(name, out next))
            {
                return null;
            }
            return next;
        }


        public void Remove(WatchingTreeItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (item.Children.Count > 0)
                throw new Exception("contains subitems");

            if (item.Parent == null)
                throw new InvalidOperationException("cannot remove root");

            WatchingTreeItem parent;
            while (true)
            {
                parent = item.Parent;
                parent.Children.Remove(item.Name);

                if (parent.Parent != null && parent.Children.Count == 0)
                    item = parent;
                else
                    break;
            }
        }

        public string PathOfItem(WatchingTreeItem item)
        {
            StringBuilder builder = new StringBuilder(256);
            WatchingTreeItem root = Root;
            while (item != root)
            {
                builder.Insert(0, item.Name);
                builder.Insert(0, Path.PathHelper.SEPERATOR);
                item = item.Parent;
            }
            if (builder.Length > 0)
                builder.Remove(0, 1);
            return builder.ToString();
        }

        internal WatchingTree()
        {
            Root = new WatchingTreeItem("/", null, ItemType.Directory);
        }
    }
}
