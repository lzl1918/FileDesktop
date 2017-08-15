using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Path
{
    public static class PathHelper
    {
        public const char SEPERATOR = '\\';

        public static bool IsSubItem(string parent, string sub, int withinDepth)
        {
            int depth = ChildDepth(parent, sub);
            return depth > 0 && depth <= withinDepth;
        }
        public static bool IsSubItem(string parent, string sub)
        {
            return ChildDepth(parent, sub) > 0;
        }
        public static int ChildDepth(string parent, string sub)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (sub == null)
                throw new ArgumentNullException(nameof(sub));

            while (parent.Length > 0 && (parent[parent.Length - 1] == '/' || parent[parent.Length - 1] == '\\'))
                parent = parent.Substring(0, parent.Length - 1);
            while (sub.Length > 0 && (sub[sub.Length - 1] == '/' || sub[sub.Length - 1] == '\\'))
                sub = sub.Substring(0, sub.Length - 1);

            if (!sub.StartsWith(parent))
                return -1;
            string post = sub.Substring(parent.Length);
            while (post.Length > 0 && (post[0] == '/' || post[0] == '\\'))
                post = post.Substring(1);

            if (post.Length == 0)
                return 0;
            string[] parents = post.Split(SEPERATOR);
            return parents.Length;
        }
        public static string GetNeareastParent(string path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            while (path.Length > 0 && (path[path.Length - 1] == '/' || path[path.Length - 1] == '\\'))
                path = path.Substring(0, path.Length - 1);

            string[] paths = path.Split(SEPERATOR);
            string parent;
            int[] splitIndices = new int[paths.Length];
            splitIndices[0] = 0;
            for (int i = 1; i < paths.Length; i++)
                splitIndices[i] = splitIndices[i - 1] + paths[i - 1].Length + 1;
            for (int i = paths.Length - 1; i >= 1; i--)
            {
                parent = path.Substring(0, splitIndices[i] - 1);
                if (Directory.Exists(parent))
                    return parent;
            }
            return null;
        }
    }
}
