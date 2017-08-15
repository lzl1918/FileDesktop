using Base.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Watcher.Tree
{
    public enum ItemWatchingStatus
    {
        None = 0x0,
        Watched = 0x1,
        Expected = 0x2,
        ScanNeeded = 0x3
    }
    public sealed class ItemWatchingOptions
    {
        public ItemWatchingStatus WatchingStatus { get; set; }
        public DirectoryScanOptions ScanOptions { get; set; }
    }
}
