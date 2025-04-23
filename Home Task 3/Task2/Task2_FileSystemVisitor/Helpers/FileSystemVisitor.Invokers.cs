using System;
using System.IO;

namespace Task2_FileSystemVisitorApp;

public partial class FileSystemVisitor
{
    // Event invokers
    protected virtual void OnStarted() => Started?.Invoke(this, EventArgs.Empty);
    protected virtual void OnFinished() => Finished?.Invoke(this, EventArgs.Empty);

    protected virtual void OnFileFound(FileSystemEventArgs e)
    {
        FileFound?.Invoke(this, e);
    }

    protected virtual void OnDirectoryFound(FileSystemEventArgs e)
    {
        DirectoryFound?.Invoke(this, e);
    }

    protected virtual void OnFilteredFileFound(FileSystemEventArgs e)
    {
        FilteredFileFound?.Invoke(this, e);
    }

    protected virtual void OnFilteredDirectoryFound(FileSystemEventArgs e)
    {
        FilteredDirectoryFound?.Invoke(this, e);
    }
}
