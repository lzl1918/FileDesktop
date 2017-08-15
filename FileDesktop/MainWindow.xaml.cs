using Base.Configurations;
using Base.StorageItems;
using Base.Watcher;
using FileDesktop.Helpers;
using FileDesktop.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileDesktop
{
    public partial class MainWindow : Window
    {
        private WatcherManager watcherManager = null;
        private List<ItemInfo> rawItems;
        private ObservableCollection<ItemInfo> currentItems;
        private System.Windows.Forms.NotifyIcon tray = null;
        public MainWindow()
        {
            rawItems = new List<ItemInfo>();

            InitializeComponent();

            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisposeCurrentWatcherManager();
        }



        private void ExecuteSearch(string search)
        {
            if (search.Length <= 0)
            {
                foreach (ItemInfo item in currentItems)
                {
                    item.DisplayName.ClearHighlight();
                    item.DisplayPath.ClearHighlight();
                }
                currentItems.Clear();
                foreach (ItemInfo item in rawItems)
                    currentItems.Add(item);
                return;
            }
            search = search.ToLower();
            if (search[0] == '>')
            {
                search = search.Substring(1);
                if (search.Length <= 0)
                    return;
                ExecutePathSearch(search);
            }
            else
            {
                ExecuteNameSearch(search);
            }
        }
        private void ExecutePathSearch(string search)
        {

        }
        private void ExecuteNameSearch(string search)
        {
            List<ItemInfo> searchResult = new List<ItemInfo>();
            string name;
            int index;
            Match match;
            Regex matchReg = null;
            try
            {
                matchReg = new Regex(search);
            }
            catch
            {
                matchReg = null;
            }
            if (matchReg != null)
            {
                foreach (ItemInfo itemInfo in rawItems)
                {
                    name = itemInfo.LowerName;
                    if ((index = name.IndexOf(search)) >= 0)
                    {
                        itemInfo.DisplayName.UpdateRange(index, search.Length);
                        searchResult.Add(itemInfo);
                    }
                    else
                    {
                        match = matchReg.Match(name);
                        if (match.Success)
                        {
                            itemInfo.DisplayName.UpdateRange(match.Index, match.Length);
                            searchResult.Add(itemInfo);
                        }
                    }
                }
            }
            else
            {
                foreach (ItemInfo itemInfo in rawItems)
                {
                    name = itemInfo.LowerName;
                    if ((index = name.IndexOf(search)) >= 0)
                    {
                        name = itemInfo.Name.Substring(0, index) + '<' + itemInfo.Name.Substring(index, search.Length) + '>' + itemInfo.Name.Substring(index + search.Length);
                        itemInfo.DisplayName.UpdateRange(index, search.Length);
                        searchResult.Add(itemInfo);
                    }
                }
            }
            currentItems.Clear();
            foreach (ItemInfo item in searchResult)
                currentItems.Add(item);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            Rect primaryArea = SystemParameters.WorkArea;
            double offsetTop = primaryArea.Top;
            double offsetLeft = primaryArea.Right - this.Width;
            double panelHeight = primaryArea.Height;
            this.Left = offsetLeft;
            this.Height = panelHeight;
            this.Top = offsetTop;

            WindowInteropHelper interopHelper = new WindowInteropHelper(this);
            GlassHelper.EnableAero(interopHelper.Handle);
            LoadTray();

            LoadItemsAsync();
        }
        private void LoadTray()
        {
            tray = new System.Windows.Forms.NotifyIcon();
            Assembly assembly = Assembly.GetEntryAssembly();
            Stream iconStream = assembly.GetManifestResourceStream("FileDesktop.tray.ico");
            tray.Icon = new System.Drawing.Icon(iconStream);
            iconStream.Close();
            iconStream.Dispose();
            tray.Visible = true;
            System.Windows.Forms.MenuItem editConfigMenu = new System.Windows.Forms.MenuItem("编辑配置", (sender, e) =>
            {
                string configurationFilePath = App.ConfigurationFilePath;
                if (File.Exists(configurationFilePath))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo("vscode", $"\"{configurationFilePath}\"");
                    Process.Start(startInfo);
                }
            });
            System.Windows.Forms.MenuItem reloadMenu = new System.Windows.Forms.MenuItem("刷新配置", (sender, e) =>
            {
                SynchronizationContext context = SynchronizationContext.Current;
                (sender as System.Windows.Forms.MenuItem).Enabled = false;
                LoadItemsAsync().ContinueWith(tsk =>
                {
                    context.Send(state =>
                    {
                        (sender as System.Windows.Forms.MenuItem).Enabled = true;
                    }, null);
                });
            });
            System.Windows.Forms.MenuItem closeMenu = new System.Windows.Forms.MenuItem("关闭", (sender, e) =>
            {
                tray.Visible = false;
                tray.Dispose();
                Application.Current.Shutdown();
            });
            System.Windows.Forms.MenuItem[] menuitems = new System.Windows.Forms.MenuItem[] { editConfigMenu, reloadMenu, closeMenu };
            tray.ContextMenu = new System.Windows.Forms.ContextMenu(menuitems);
        }

        private Task LoadItemsAsync()
        {
            IConfiguartion configuration = ConfigurationHelper.ReadConfiguration();
            SynchronizationContext context = SynchronizationContext.Current;
            return Task.Run<ItemsResult>(() =>
            {
                return configuration.EnumerateItems();
            }).ContinueWith(task =>
            {
                ItemsResult enumerateResult = task.Result;
                Task retriveTask = null;
                context.Send(state =>
                {
                    retriveTask = ItemInfo.RetriveItemInfosAsync(enumerateResult).ContinueWith(tsk =>
                     {
                         Dispatcher.BeginInvoke((Action)(() =>
                         {
                             rawItems.Clear();
                             rawItems.AddRange(tsk.Result);
                             currentItems = new ObservableCollection<ItemInfo>(rawItems);
                             listview_items.ItemsSource = currentItems;
                         }));
                     });
                }, null);
                retriveTask.Wait();

                // watchers
                LoadWatchers(enumerateResult);
            });
        }
        private void LoadWatchers(ItemsResult result)
        {
            DisposeCurrentWatcherManager();

            watcherManager = WatcherManager.CreateFromTree(result.WatchingTree);
            watcherManager.ItemAdded += OnItemAdded;
            watcherManager.ItemRemoved += OnItemRemoved;
            watcherManager.BeginWatch();
        }
        private void DisposeCurrentWatcherManager()
        {
            if (watcherManager == null)
                return;
            watcherManager.ItemAdded -= OnItemAdded;
            watcherManager.ItemRemoved -= OnItemRemoved;
            watcherManager.Dispose();
            watcherManager = null;
        }
        private void OnItemRemoved(object sender, WatchingItemEventArgs e)
        {
            int index = 0;
            int count = rawItems.Count;
            ItemInfo item;
            for (; index < count; index++)
            {
                item = rawItems[index];
                if (item.Name == e.Name && item.FullPath == e.Path && item.Type == e.Type)
                    break;
            }
            if (index < count)
                rawItems.RemoveAt(index);

            index = 0;
            count = currentItems.Count;
            for (; index < count; index++)
            {
                item = currentItems[index];
                if (item.Name == e.Name && item.FullPath == e.Path && item.Type == e.Type)
                    break;
            }
            if (index < count)
            {
                Dispatcher.Invoke(() =>
                {
                    currentItems.RemoveAt(index);
                });
            }
        }

        private void OnItemAdded(object sender, WatchingItemEventArgs e)
        {
            int directoryCount = 0;
            int count = rawItems.Count;
            for (; directoryCount < count; directoryCount++)
                if (rawItems[directoryCount].Type == ItemType.File)
                    break;

            int max = 0;
            int indexofRaw = 0;
            if (e.Type == ItemType.Directory)
            {
                max = directoryCount;
                indexofRaw = 0;
            }
            else
            {
                max = count;
                indexofRaw = directoryCount;
            }
            for (; indexofRaw < max; indexofRaw++)
            {
                if (rawItems[indexofRaw].Name.CompareTo(e.Name) < 1)
                    continue;
                else
                    break;
            }
            
            Dispatcher.Invoke(() =>
            {
                ItemInfo item = ItemInfo.FromPath(e.Path, e.Type);
                if (rawItems.Count == currentItems.Count)
                    currentItems.Insert(indexofRaw, item);
                rawItems.Insert(indexofRaw, item);
            });

        }

        private void OnItemDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            ListView listView = sender as ListView;
            ItemInfo itemInfo = listView.SelectedItem as ItemInfo;
            if (itemInfo == null)
                return;

            ProcessStartInfo startInfo = new ProcessStartInfo(itemInfo.FullPath);
            Process.Start(startInfo);
        }

        private void OpenContainingFolderClicked(object sender, RoutedEventArgs e)
        {
            ItemInfo itemInfo = listview_items.SelectedItem as ItemInfo;
            if (itemInfo == null)
                return;

            string parentPath = null;
            if (itemInfo.Type == ItemType.Directory)
            {
                DirectoryInfo dir = new DirectoryInfo(itemInfo.FullPath);
                parentPath = dir.Parent.FullName;
            }
            else
            {
                FileInfo file = new FileInfo(itemInfo.FullPath);
                parentPath = file.Directory.FullName;
            }
            if (parentPath != null)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(parentPath);
                Process.Start(startInfo);
            }
        }
        private void OpenInVSCodeClicked(object sender, RoutedEventArgs e)
        {
            ItemInfo itemInfo = listview_items.SelectedItem as ItemInfo;
            if (itemInfo == null)
                return;
            ProcessStartInfo startInfo = new ProcessStartInfo("vscode", $"\"{itemInfo.FullPath}\"");
            Process.Start(startInfo);
        }
        private void OpenContainingFolderInVSCodeClicked(object sender, RoutedEventArgs e)
        {
            ItemInfo itemInfo = listview_items.SelectedItem as ItemInfo;
            if (itemInfo == null)
                return;

            string parentPath = null;
            if (itemInfo.Type == ItemType.Directory)
            {
                DirectoryInfo dir = new DirectoryInfo(itemInfo.FullPath);
                parentPath = dir.Parent.FullName;
            }
            else
            {
                FileInfo file = new FileInfo(itemInfo.FullPath);
                parentPath = file.Directory.FullName;
            }
            if (parentPath != null)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo("vscode", $"\"{parentPath}\"");
                Process.Start(startInfo);
            }
        }

        private void OnSearchBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null)
                return;
            string search = textBox.Text;
            ExecuteSearch(search);
        }
    }
}
