# FileDesktop

Show your files and folders in desktop

## Options
Path to configuration file `config.json` should be passed to the program when it starts as the first argument. Or the path `"(working directory)\config.json"` will be used.

Options in the configuration file are used to determine what items should be displayed in the main interface.

Configurable options are introduced below.

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