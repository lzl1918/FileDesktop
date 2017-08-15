## Options
Configuration file `config.json` should be put to working directory of this program.

### Object Model
```CSharp
class ScanOption {
    string Path;
    int Depth;
    FilterOption Include;
    FilterOption Exclude;
}
class FilterOption {
    List<Regex> File;
    List<Regex> Directory;
    List<Regex> Common;
    List<Regex> Path;
}
class IncludeOption {
    List<string> Files;
    List<string> Directories;
}
class Items {
    List<ScanOption> Scan;
    IncludeOption Include;
    FilterOption Exclude
}
```
### Items
Contains configurations.
### FilterOption
Determines how to filtrate directories and files.

- Regular expressiones in `file` are applied to name of each file.
- Regular expressiones in `directories` are applied to name of each directory.
- Regular expressiones in `common` are applied to name of both files and directories.
- Regular expressiones in `path` are applied to full path of files or directories.

`FilterOption` can be used as either `include` option or `exclude` option.
### ScanOption

### IncludeOption
Which files and directories should be added directly.