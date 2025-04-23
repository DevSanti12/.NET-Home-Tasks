using System;

namespace Task2_FileSystemVisitorApp;

public class FileSystemEventArgs : EventArgs
{
    public string Path { get; }
    public bool AbortSearch { get; set; }
    public bool Exclude { get; set; }

    public FileSystemEventArgs(string path)
    {
        Path = path;
    }
}
