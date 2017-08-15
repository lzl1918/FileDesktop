using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FileDesktop.Helpers
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct SHFILEINFO
    {
        public IntPtr hIcon;
        public IntPtr iIcon;
        public uint aattributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string displayname;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string typename;
    }

    public static class IconHelper
    {
        [DllImport("shell32.dll", EntryPoint = "SHGetFileInfo")]
        internal static extern IntPtr SHGetFileInfo(string path, uint fileattribute, ref SHFILEINFO sfinfo, uint SizeFileInfo, uint flag);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon")]
        internal static extern int DestroyIcon(IntPtr hIcon);

        public static BitmapImage ReadIcon(string path)
        {
            SHFILEINFO shFileInfo = new SHFILEINFO();
            IntPtr retPtr = SHGetFileInfo(path, 0x80, ref shFileInfo, (uint)Marshal.SizeOf(shFileInfo), 0x100);
            if (retPtr == IntPtr.Zero)
                return null;

            Icon icon = (Icon)Icon.FromHandle(shFileInfo.hIcon).Clone();
            DestroyIcon(shFileInfo.hIcon);
            Bitmap iconBitmap = icon.ToBitmap();
            BitmapImage image = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                iconBitmap.Save(stream, ImageFormat.Png);
                image.BeginInit();
                image.StreamSource = stream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
            }
            return image;
        }
    }
}
