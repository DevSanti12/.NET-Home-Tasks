using System;

namespace Task1_FileSystemVisitorApp;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("** Welcome to the File System Visitor Program! (Task1) ** \n");
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
            Console.WriteLine("All files with filter: ");
            foreach (var file in visitorWithFileFilter.Traverse())
            {
                Console.WriteLine($"Found: {file}");
            }
        }
        else if (filterOption.ToLower() == "folder")
        {
            Console.WriteLine("What name of the folder to search for: ");
            string folderName = Console.ReadLine();
            var visitorWithFolderFilter = new FileSystemVisitor(beginningPath, folder => folder.EndsWith(folderName));
            Console.WriteLine("All folders with filter: ");
            foreach (var folder in visitorWithFolderFilter.Traverse())
            {
                Console.WriteLine($"Found: {folder}");
            }
        }
        else
        {
            var visitor = new FileSystemVisitor(beginningPath);
            Console.WriteLine("All folders and files: ");
            foreach (var folder in visitor.Traverse())
            {
                Console.WriteLine($"Found: {folder}");
            }
        }
    }
}