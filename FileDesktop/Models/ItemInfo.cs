using Base.Configurations;
using Base.StorageItems;
using Base.Watcher;
using FileDesktop.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace FileDesktop.Models
{

    public sealed class HighlightText
    {
        public string Text { get; set; }
        public string HighlightBefore { get; set; }
        public string Highlighted { get; set; }
        public string HighlightAfter { get; set; }

        public HighlightText(string text)
        {
            Text = text;
            HighlightBefore = text;
            Highlighted = "";
            HighlightAfter = "";
        }

        public void UpdateRange(int index, int length)
        {
            HighlightBefore = Text.Substring(0, index);
            Highlighted = Text.Substring(index, length);
            HighlightAfter = Text.Substring(index + length);
        }
        public void ClearHighlight()
        {
            HighlightBefore = Text;
            Highlighted = "";
            HighlightAfter = "";
        }
    }
    public sealed class ItemInfo
    {
        public HighlightText DisplayName { get; set; }
        public HighlightText DisplayPath { get; set; }

        public string Name { get; set; }
        public string FullPath { get; set; }

        public string LowerName { get; set; }
        public string LowerPath { get; set; }

        public BitmapImage Icon { get; set; }
        public ItemType Type { get; set; }

        private static ItemInfo FromFile(FileInfo fileInfo)
        {
            return new ItemInfo()
            {
                Name = fileInfo.Name,
                FullPath = fileInfo.FullName,
                Type = ItemType.File,
                DisplayName = new HighlightText(fileInfo.Name),
                DisplayPath = new HighlightText(fileInfo.FullName),
                LowerName = fileInfo.Name.ToLower(),
                LowerPath = fileInfo.Name.ToLower()
            };
        }
        private static ItemInfo FromDirectory(DirectoryInfo directoryInfo)
        {
            return new ItemInfo()
            {
                Name = directoryInfo.Name,
                FullPath = directoryInfo.FullName,
                Type = ItemType.Directory,
                DisplayName = new HighlightText(directoryInfo.Name),
                DisplayPath = new HighlightText(directoryInfo.FullName),
                LowerName = directoryInfo.Name.ToLower(),
                LowerPath = directoryInfo.Name.ToLower()
            };
        }

        public static Task<List<ItemInfo>> RetriveItemInfosAsync(ItemsResult result)
        {
            SynchronizationContext context = SynchronizationContext.Current;
            return Task.Run<List<ItemInfo>>(() =>
            {
                List<ItemInfo> infos = new List<ItemInfo>();
                infos.AddRange(result.Files.Select(fileInfo => ItemInfo.FromFile(fileInfo)));
                infos.AddRange(result.Directories.Select(directoryInfo => ItemInfo.FromDirectory(directoryInfo)));
                infos.Sort((a, b) =>
                {
                    if (a.Type == ItemType.Directory)
                    {
                        if (b.Type == ItemType.Directory)
                            return a.Name.CompareTo(b.Name);
                        else
                            return -1;
                    }
                    else
                    {
                        if (b.Type == ItemType.Directory)
                            return 1;
                        else
                            return a.Name.CompareTo(b.Name);
                    }
                });
                context.Send(state =>
                {
                    foreach (ItemInfo info in infos)
                        info.Icon = IconHelper.ReadIcon(info.FullPath);
                }, null);
                return infos;
            });
        }

        public static ItemInfo FromPath(string path, ItemType type)
        {
            FileSystemInfo info = null;
            if (type == ItemType.Directory)
                info = new DirectoryInfo(path);
            else
                info = new FileInfo(path);

            return new ItemInfo()
            {
                Name = info.Name,
                FullPath = info.FullName,
                Type = type,
                DisplayName = new HighlightText(info.Name),
                DisplayPath = new HighlightText(info.FullName),
                LowerName = info.Name.ToLower(),
                LowerPath = info.Name.ToLower(),
                Icon = IconHelper.ReadIcon(info.FullName)
            };
        }
    }
}
