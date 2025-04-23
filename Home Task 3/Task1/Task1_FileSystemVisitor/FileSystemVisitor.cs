using System;
using System.Collections.Generic;
using System.IO;

namespace Task1_FileSystemVisitorApp;

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

    // Iterator method to traverse and yield all files and folders
    public IEnumerable<string> Traverse()
    {
        // Use a queue to implement breadth-first traversal
        var foldersQueue = new Queue<string>();
        foldersQueue.Enqueue(_startFolder);

        while (foldersQueue.Count > 0)
        {
            var currentFolder = foldersQueue.Dequeue();

            if (_directoryFilter == null || _directoryFilter(currentFolder))
            {
                yield return currentFolder; // Yield if there's no filter or directory doesn't pass the filter
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
                if ((_fileFilter == null || _fileFilter(file)))
                {
                    yield return file;
                }
            }        
        }
    }
}
