using Base.Path;
using Base.StorageItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Configurations
{

    public sealed class DirectoryScanOptions
    {
        public string DirectoryPath { get; }
        public int ScanDepth { get; }
        public ScanFilterOptions Includes { get; }
        public ScanFilterOptions Excludes { get; }

        internal DirectoryScanOptions(string directoryPath, int scanDepth, ScanFilterOptions includes, ScanFilterOptions excludes)
        {
            if (directoryPath == null)
                throw new ArgumentNullException(nameof(directoryPath));
            if (includes == null)
                throw new ArgumentNullException(nameof(includes));
            if (excludes == null)
                throw new ArgumentNullException(nameof(excludes));
            if (scanDepth < 0)
                throw new ArgumentOutOfRangeException(nameof(scanDepth));

            DirectoryPath = directoryPath;
            ScanDepth = scanDepth;
            Includes = includes;
            Excludes = excludes;
        }

        public bool IsIncluded(string name, string path, ItemType type)
        {
            int depth = PathHelper.ChildDepth(DirectoryPath, path);
            return Includes.IsMatch(name, path, type) && !Excludes.IsMatch(name, path, type) && depth > 0 && depth <= ScanDepth;
        }
    }
}
