using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task2_FileSystemVisitorApp;

public static class SubcribeToEvents
{
    public static void Subscribe(FileSystemVisitor fsv)
    {
        fsv.Started += (sender, eventArgs) => Console.WriteLine("Search started!");
        fsv.Finished += (sender, eventArgs) => Console.WriteLine("Search finished!");

        fsv.FileFound += (sender, e) =>
        {

            if (Path.GetFileName(e.Path).EndsWith("temp"))
            {
                e.Exclude = true;
            }
            else
            {
                Console.WriteLine($"Event File: {e.Path}");
            }
        };

        fsv.DirectoryFound += (sender, e) =>
        {
            if (Path.GetDirectoryName(e.Path).StartsWith("PRIVATE"))
            {
                e.Exclude = true;
            }
            else
            {
                Console.WriteLine($"Event Directory: {e.Path}");
            }
        };

        fsv.FilteredFileFound += (sender, e) =>
        {
            Console.WriteLine($"** Event Filtered file found!: {e.Path}  **");
        };

        fsv.FilteredDirectoryFound += (sender, e) =>
        {
            Console.WriteLine($"** Event Filtered directory found!: {e.Path}  **");
            Console.WriteLine("Aborting Search!!!!");
            e.AbortSearch = true; //no need to continue looking
        };
    }
}
