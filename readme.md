# FileDesktop

Show your files and folders in desktop

![demo](https://github.com/lzl1918/FileDesktop/blob/master/sample.png)

## Watchers
[FileSystemWatcher](https://msdn.microsoft.com/library/system.io.filesystemwatcher.aspx) is referenced to detect changes of file system to add or remove items in time.

**Watched directory should not include the current program as subitem or any change of monitored content will not raise events.**

## Options
Path to configuration file `config.json` should be passed to the program when it starts as the first argument. Or the path `"(working directory)\config.json"` will be used.

Options in the configuration file are used to determine what items should be displayed in the main interface.

Configurable options are introduced below.

Sample of options can be referenced at [default.json](https://github.com/lzl1918/FileDesktop/blob/master/FileDesktop/default.json)

[ConfigurationReader.cs](https://github.com/lzl1918/FileDesktop/blob/master/FileDesktop.Base/Configurations/ConfigurationReader.cs) shows the way that the options be read.


#### default of include options
Include all items
```Json
{
    "file": [],
    "directory": [],
    "common": [ "^" ],
    "path": []
}
```

#### default of exclude options
Exclude none item
```Json
{
    "file": [],
    "directory": [],
    "common": [],
    "path": []
}
```

### Object Model
```CSharp
interface IConfiguration
{
    // patterns that used to exclude files and folders by name or path
    ScanFilterOptions GlobalExclude {get;}

    // list of explicitly included files or folders
    ItemIncludeOptions IncludedItems {get;}

    // scan options that claim the operations how the program scans items under specific directories
    List<DirectoryScanOptions> ScanDirectories {get;}
}

class ScanFilterOptions {
    // patterns that filter files by FileName
    IReadOnlyList<Regex> FileFilters {get;}

    // patterns that filter directories by DirectoryName
    IReadOnlyList<Regex> DirectoryFilters {get;}

    // patterns that filter files or directories by its name
    IReadOnlyList<Regex> CommonFilters {get;}

    // patterns that filter items by the full path
    IReadOnlyList<Regex> PathFilters {get;}
}

class ItemIncludeOptions {
    // path to directories that should be included
    IReadOnlyList<string> Directories {get;}

    // path to files
    IReadOnlyList<string> Files {get;}
}

class DirectoryScanOptions {
    // path of scanned directory
    string DirectoryPath {get;}

    // depth that the program scans within the folder
    int ScanDepth {get;}

    // patterns that determine excluded items
    ScanFilterOptions Excludes {get;}

    // patterns that explicitly include items that may be ignored by exclude options
    ScanFilterOptions Includes {get;}
}
```
