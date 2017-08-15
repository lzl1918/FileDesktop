using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FileDesktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string ConfigurationFilePath { get; private set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                ConfigurationFilePath = e.Args[0];
            }
            else
            {
                ConfigurationFilePath = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            }
            base.OnStartup(e);

        }
    }
}
