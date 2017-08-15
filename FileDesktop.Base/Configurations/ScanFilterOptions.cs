using Base.StorageItems;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Base.Configurations
{
    public sealed class ScanFilterOptions
    {
        public bool IsDefault { get; private set; }
        public IReadOnlyList<Regex> FileFilters { get; }
        public IReadOnlyList<Regex> DirectoryFilters { get; }
        public IReadOnlyList<Regex> CommonFilters { get; }
        public IReadOnlyList<Regex> PathFilters { get; }

        internal ScanFilterOptions(IEnumerable<Regex> fileFilters, IEnumerable<Regex> directoryFilters, IEnumerable<Regex> commonFilters, IEnumerable<Regex> pathFilters)
        {
            if (fileFilters == null)
                throw new ArgumentNullException(nameof(fileFilters));
            if (directoryFilters == null)
                throw new ArgumentNullException(nameof(directoryFilters));
            if (commonFilters == null)
                throw new ArgumentNullException(nameof(commonFilters));
            if (pathFilters == null)
                throw new ArgumentNullException(nameof(pathFilters));

            List<Regex> files = new List<Regex>(fileFilters);
            List<Regex> directories = new List<Regex>(directoryFilters);
            List<Regex> commons = new List<Regex>(commonFilters);
            List<Regex> paths = new List<Regex>(pathFilters);

            FileFilters = files;
            DirectoryFilters = directories;
            CommonFilters = commons;
            PathFilters = paths;
            IsDefault = false;
        }

        internal void Combine(ScanFilterOptions option)
        {
            if (option == null)
                throw new ArgumentNullException(nameof(option));

            (FileFilters as List<Regex>).AddRange(option.FileFilters);
            (DirectoryFilters as List<Regex>).AddRange(option.DirectoryFilters);
            (CommonFilters as List<Regex>).AddRange(option.CommonFilters);
            (PathFilters as List<Regex>).AddRange(option.PathFilters);
            IsDefault &= option.IsDefault;
        }


        internal static ScanFilterOptions CreateExcludeDefault()
        {
            List<Regex> fileFilters = new List<Regex>();
            List<Regex> directoryFilters = new List<Regex>();
            List<Regex> commonFilters = new List<Regex>();
            List<Regex> pathFilters = new List<Regex>();
            return new ScanFilterOptions(fileFilters, directoryFilters, commonFilters, pathFilters)
            {
                IsDefault = true
            };
        }
        internal static ScanFilterOptions CreateIncludeDefault()
        {
            List<Regex> fileFilters = new List<Regex>();
            List<Regex> directoryFilters = new List<Regex>();
            List<Regex> commonFilters = new List<Regex>() { new Regex("^") };
            List<Regex> pathFilters = new List<Regex>();
            return new ScanFilterOptions(fileFilters, directoryFilters, commonFilters, pathFilters)
            {
                IsDefault = true
            };
        }

        public bool IsMatch(FileSystemInfo itemInfo)
        {
            string path = itemInfo.FullName;
            if (PathFilters.Any(reg => reg.IsMatch(path)))
                return true;

            string name = itemInfo.Name;
            if (CommonFilters.Any(reg => reg.IsMatch(name)))
                return true;

            if (itemInfo is FileInfo fileInfo)
                return FileFilters.Any(reg => reg.IsMatch(name));
            else if (itemInfo is DirectoryInfo directoryInfo)
                return DirectoryFilters.Any(reg => reg.IsMatch(name));
            else
                return false;
        }
        public bool IsMatch(string name, string path, ItemType type)
        {
            if (PathFilters.Any(reg => reg.IsMatch(path)))
                return true;

            if (CommonFilters.Any(reg => reg.IsMatch(name)))
                return true;

            if (type == ItemType.File)
                return FileFilters.Any(reg => reg.IsMatch(name));
            else if (type == ItemType.Directory)
                return DirectoryFilters.Any(reg => reg.IsMatch(name));
            else
                return false;
        }
    }
}
