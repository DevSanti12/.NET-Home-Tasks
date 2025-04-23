using System;
using System.ComponentModel;

namespace Task2_FileSystemVisitorApp;

public class Program
{

    public static void Main(string[] args)
    {
        Console.WriteLine("** Welcome to the File System Visitor Program! (Task 2) ** \n");
        Console.WriteLine("Enter the path of the starting folder: ");

        var beginningPath = Console.ReadLine();
        while (!RegexContainer.ValidatePathFormat(beginningPath))
        {
            Console.WriteLine($"Please enter a valid path, path {beginningPath} is not valid");
            beginningPath = Console.ReadLine();
        }

        Console.WriteLine("Would you like to filter for only a specific folder or file? : ");
        string filterOption = Console.ReadLine();

        if (filterOption.ToLower() == "file")
        {
            Console.WriteLine("What extension of the file to search for? (txt ,pdf, jpg) : ");
            string extension = Console.ReadLine();
            extension = extension.StartsWith(".") ? extension.Substring(1) : extension;

            var visitorWithFileFilter = new FileSystemVisitor(beginningPath, path => path.EndsWith($".{extension}"));
            SubcribeToEvents.Subscribe(visitorWithFileFilter);
            Console.WriteLine("All folders and files with file filter: ");
            
            // Create a list to store the resulting files
            var filteredFiles = new List<string>();
            foreach (var file in visitorWithFileFilter.Traverse())
            {
                // Add the files to a list or array
                filteredFiles.Add(file);
            }
            PrintFilteredItems(filteredFiles, filterOption.ToLower());
        }
        else if (filterOption.ToLower() == "folder")
        {
            Console.WriteLine("What name of the folder to search for (ONCE FOUND, SEARCH WILL END): ");
            string folderName = Console.ReadLine();
            var visitorWithFolderFilter = new FileSystemVisitor(beginningPath, folder => folder.EndsWith(folderName));

            SubcribeToEvents.Subscribe(visitorWithFolderFilter);
            Console.WriteLine("All folders and files with file filter: ");

            var filteredFolders = new List<string>();
            foreach (var folder in visitorWithFolderFilter.Traverse())
            {
                filteredFolders.Add(folder);
            }
            PrintFilteredItems(filteredFolders, filterOption.ToLower());
        }
        else
        {
            var visitor = new FileSystemVisitor(beginningPath);
            SubcribeToEvents.Subscribe(visitor);
            Console.WriteLine("All folders and files: ");
            foreach (var folder in visitor.Traverse()) {}
        } 
    }

    public static void PrintFilteredItems(IEnumerable<string> items, string Option)
    {
        Console.WriteLine($"\n All the filtered {Option}s");
        foreach (var item in items) { Console.WriteLine(item); }
    }
}