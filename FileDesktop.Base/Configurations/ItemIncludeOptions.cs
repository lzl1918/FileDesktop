using System;
using System.Collections.Generic;

namespace Base.Configurations
{
    public sealed class ItemIncludeOptions
    {
        public IReadOnlyList<string> Directories { get; }
        public IReadOnlyList<string> Files { get; }

        internal ItemIncludeOptions(IReadOnlyList<string> directories, IReadOnlyList<string> files)
        {
            if (directories == null)
                throw new ArgumentNullException(nameof(directories));
            if (files == null)
                throw new ArgumentNullException(nameof(files));

            Directories = directories;
            Files = files;
        }
    }
}
