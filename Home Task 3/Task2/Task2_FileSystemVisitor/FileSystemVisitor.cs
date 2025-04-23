using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace Task2_FileSystemVisitorApp;

public partial class FileSystemVisitor
{
    private readonly string _startFolder;
    private readonly Func<string, bool>? _fileFilter;       // Filter for files
    private readonly Func<string, bool>? _directoryFilter; // Filter for directories
    private readonly IFileSystem _fileSystem; //file system dependency

    //events for the 4 types of notifications
    public event EventHandler Started;
    public event EventHandler Finished;
    public event EventHandler<FileSystemEventArgs> FileFound;
    public event EventHandler<FileSystemEventArgs> DirectoryFound;
    public event EventHandler<FileSystemEventArgs> FilteredFileFound;
    public event EventHandler<FileSystemEventArgs> FilteredDirectoryFound;

    // Constructor without a filter
    public FileSystemVisitor(string startFolder, IFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));

        if (string.IsNullOrEmpty(startFolder))
            throw new ArgumentException("Starting folder cannot be null or empty", nameof(startFolder));

        if (!_fileSystem.Directory.Exists(startFolder))
            throw new DirectoryNotFoundException($"The directory '{startFolder}' does not exist.");

        _startFolder = startFolder;
        _fileFilter = null;       // No file filter 
        _directoryFilter = null; // No directory filter
    }

    public FileSystemVisitor(string startFolder, IFileSystem fileSystem, Func<string, bool> filter)
        : this(startFolder, fileSystem)
    {
        _fileFilter = filter;
        _directoryFilter = filter;
    }

    public FileSystemVisitor(string startFolder, IFileSystem fileSystem, Func<string, bool> fileFilter, Func<string, bool> directoryFilter)
        : this(startFolder, fileSystem)
    {
        _fileFilter = fileFilter;
        _directoryFilter = directoryFilter;
    }

    // Iterator method to traverse and yield all files and folders
    public IEnumerable<string> Traverse()
    {
        // Use a queue to implement breadth-first traversal
        var foldersQueue = new Queue<string>();
        foldersQueue.Enqueue(_startFolder);

        //started event
        OnStarted();

        while (foldersQueue.Count > 0)
        {
            var currentFolder = foldersQueue.Dequeue();

            // Notify about the directory found
            var directoryArgs = new FileSystemEventArgs(currentFolder);

            if (!directoryArgs.Exclude) // Work on non-excluded directories
            {
                if (_directoryFilter == null || !_directoryFilter(currentFolder))
                {
                    //event invoke
                    OnDirectoryFound(directoryArgs);
                }
                else
                {
                    // If filtering is enabled and directory matches the filter
                    var filteredDirArgs = new FileSystemEventArgs(currentFolder);
                    
                    //Event occured
                    OnFilteredDirectoryFound(filteredDirArgs);
                    
                    yield return currentFolder;

                    //Only will abort if the specified folder is found
                    //This is to use the abort property in FileSystemEventArgs
                    if (filteredDirArgs.AbortSearch)
                    {
                        break;
                    }

                }
            }


            // Try to gather all subdirectories
            IEnumerable<string> subdirectories;
            try
            {
                subdirectories = _fileSystem.Directory.GetDirectories(currentFolder);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to folder: {currentFolder}");
                continue;
            }

            foreach (var subdirectory in subdirectories)
            {
                foldersQueue.Enqueue(subdirectory);
            }

            // Try to gather all files in the current directory
            IEnumerable<string> files;
            try
            {
                files = _fileSystem.Directory.GetFiles(currentFolder);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"Access denied to folder: {currentFolder}");
                continue;
            }

            foreach (var file in files)
            {

                // Notify about the file found
                var fileArgs = new FileSystemEventArgs(file);

                if (!fileArgs.Exclude) // Work on non-excluded files
                {
                    if (!fileArgs.Exclude && (_fileFilter == null || !_fileFilter(file)))
                    {
                        //Event invoke
                        OnFileFound(fileArgs);
                    }
                    else
                    {
                        OnFilteredFileFound(fileArgs);
                        yield return file;
                    }
                }
            }
        }
        //finished event
        OnFinished();
    }
}
