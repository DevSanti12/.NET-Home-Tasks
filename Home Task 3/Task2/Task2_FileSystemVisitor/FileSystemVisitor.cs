using System;
using System.Collections.Generic;
using System.IO;

namespace Task2_FileSystemVisitorApp;

public partial class FileSystemVisitor
{
    private readonly string _startFolder;
    private readonly Func<string, bool> _fileFilter;       // Filter for files
    private readonly Func<string, bool> _directoryFilter; // Filter for directories

    //events for the 4 types of notifications
    public event EventHandler Started;
    public event EventHandler Finished;
    public event EventHandler<FileSystemEventArgs> FileFound;
    public event EventHandler<FileSystemEventArgs> DirectoryFound;
    public event EventHandler<FileSystemEventArgs> FilteredFileFound;
    public event EventHandler<FileSystemEventArgs> FilteredDirectoryFound;

    private bool _abortSearch; // Flag to abort search

    // Constructor without a filter
    public FileSystemVisitor(string startFolder)
    {
        if (string.IsNullOrEmpty(startFolder))
            throw new ArgumentException("Starting folder cannot be null or empty", nameof(startFolder));

        if (!Directory.Exists(startFolder))
            throw new DirectoryNotFoundException($"The directory '{startFolder}' does not exist.");

        _startFolder = startFolder;
        _fileFilter = null;       // No file filter 
        _directoryFilter = null; // No directory filter
    }

    public FileSystemVisitor(string startFolder, Func<string, bool> filter)
        : this(startFolder)
    {
        _fileFilter = filter;
        _directoryFilter = filter;
    }

    public FileSystemVisitor(string startFolder, Func<string, bool> fileFilter, Func<string, bool> directoryFilter)
        : this(startFolder)
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
            if (_abortSearch)
                break; // Abort search if requested

            var currentFolder = foldersQueue.Dequeue();

            // Notify about the directory found
            var directoryArgs = new FileSystemEventArgs(currentFolder);

            if (directoryArgs.AbortSearch)
            {
                //_abortSearch = true;
                break; // Abort search if requested
            }

            if (!directoryArgs.Exclude) // Work on non-excluded directories
            {
                if (_directoryFilter == null || !_directoryFilter(currentFolder))
                {
                    OnDirectoryFound(directoryArgs);
                    //yield return currentFolder; // Yield if there's no filter or directory doesn't pass the filter
                }
                else
                {
                    // If filtering is enabled and directory matches the filter
                    var filteredDirArgs = new FileSystemEventArgs(currentFolder);
                    _abortSearch = true;
                    OnFilteredDirectoryFound(filteredDirArgs);
                    yield return currentFolder;
                    if (directoryArgs.AbortSearch)
                    {
                        //_abortSearch = true;
                        break; // Abort search if requested
                    }
                }
            }


            // Try to gather all subdirectories
            IEnumerable<string> subdirectories;
            try
            {
                subdirectories = Directory.GetDirectories(currentFolder);
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
                files = Directory.GetFiles(currentFolder);
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

                if (fileArgs.AbortSearch)
                {
                    //_abortSearch = true;
                    break; // Abort search if requested
                }
                if (!fileArgs.Exclude) // Work on non-excluded files
                {
                    if (!fileArgs.Exclude && (_fileFilter == null || !_fileFilter(file)))
                    {
                        OnFileFound(fileArgs);
                        //yield return file;
                    }
                    else
                    {
                        OnFilteredFileFound(fileArgs);
                        yield return file;
                        if (fileArgs.AbortSearch)
                        {
                            //_abortSearch = true;
                            break;
                        }
                    }
                }
            }
        }
        //finished event
        OnFinished();
    }
}
