using Base.Configurations;
using Base.Path;
using Base.StorageItems;
using Hake.Extension.ValueRecord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public class Program
    {
        private static FileSystemWatcher watcher;
        static void Main(string[] args)
        {
            watcher = new FileSystemWatcher();
            watcher.BeginInit();
            watcher.Path = "G:\\Android";
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.EndInit();
            watcher.Error += OnWatcherError;
            watcher.Created += OnWatcherCreated;
            watcher.Deleted += OnWatcherDeleted;
            watcher.Renamed += OnWatcherRenamed;
            watcher.EnableRaisingEvents = true;
            AutoResetEvent waiter = new AutoResetEvent(false);
            waiter.WaitOne();
        }

        private static void WriteChange(string type, string path)
        {
            Console.WriteLine("[{0}][{1}] {2}", DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss.fff"), type, path);
        }
        private static void OnWatcherRenamed(object sender, RenamedEventArgs e)
        {
            WriteChange("rename", e.FullPath);
        }

        private static void OnWatcherDeleted(object sender, FileSystemEventArgs e)
        {
            WriteChange("deleted", e.FullPath);
        }

        private static void OnWatcherCreated(object sender, FileSystemEventArgs e)
        {
            WriteChange("created", e.FullPath);
        }

        private static void OnWatcherError(object sender, ErrorEventArgs e)
        {
        }
    }
}
