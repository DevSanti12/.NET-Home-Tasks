using Moq;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Task2_FileSystemVisitorApp;

namespace Task2_UnitTests
{
    public class FileSystemVisitorTests
    {
        private readonly Mock<IFileSystem> mockFileSystem;
        private readonly Dictionary<string, List<string>> directories;
        private readonly Dictionary<string, List<string>> files;
        public FileSystemVisitorTests()
        {
            //Initialize folder structure
            // Mock the IFileSystem dependency
            mockFileSystem = new Mock<IFileSystem>();

            directories = new Dictionary<string, List<string>>
            {
                { "/mock/start", new List<string> { "/mock/start/dir1", "/mock/start/dir2", "/mock/start/PRIVATE" } },
                { "/mock/start/dir1", new List<string>() },
                { "/mock/start/dir2", new List<string> { "/mock/start/dir2/subdir" } },
                { "/mock/start/dir2/subdir", new List<string>() },
            };

            files = new Dictionary<string, List<string>>
            {
                { "/mock/start", new List<string> { "/mock/start/file1.txt", "/mock/start/file2.pdf" } },
                { "/mock/start/dir1", new List<string> { "/mock/start/dir1/fileA.txt" , "/mock/start/dir1/tempFile.temp" } },
                { "/mock/start/dir2", new List<string>() },
                { "/mock/start/dir2/subdir", new List<string> { "/mock/start/dir2/subdir/fileB.pdf", "/mock/start/dir2/subdir/fileC.pdf" } },
            };

            // Mock Directory.Exists to always return true for predefined folders
            mockFileSystem
                .Setup(fs => fs.Directory.Exists(It.IsAny<string>()))
                .Returns((string folder) => directories.ContainsKey(folder) || files.ContainsKey(folder));

            // Mock Directory.GetDirectories to return subdirectories for a given directory
            mockFileSystem
                .Setup(fs => fs.Directory.GetDirectories(It.IsAny<string>()))
                .Returns((string folder) => directories.ContainsKey(folder) ? directories[folder].ToArray() : Array.Empty<string>());

            // Mock Directory.GetFiles to return files for a given directory
            mockFileSystem.Setup(fs => fs.Directory.GetFiles(It.IsAny<string>()))
                .Returns((string folder) => files.ContainsKey(folder) ? files[folder].ToArray() : Array.Empty<string>());
        }

        [Fact]
        public void TestTraverseAll_ShouldTriggerEvents()
        {
            // Arrange
            var startFolder = "/mock/start";
            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object);

            var eventsTriggered = new List<string>();

            // Subscribe to events
            visitor.Started += (sender, e) => eventsTriggered.Add("Started");
            visitor.Finished += (sender, e) => eventsTriggered.Add("Finished");
            visitor.DirectoryFound += (sender, e) => eventsTriggered.Add($"DirectoryFound: {e.Path}");
            visitor.FileFound += (sender, e) => eventsTriggered.Add($"FileFound: {e.Path}");

            // Act
            var results = visitor.Traverse().ToList();

            // Assert events
            // No files are yielded since no filter exists
            Assert.Empty(results);
            Assert.Contains("Started", eventsTriggered);
            Assert.Contains("Finished", eventsTriggered);
            Assert.Contains("DirectoryFound: /mock/start/dir1", eventsTriggered);
            Assert.Contains("FileFound: /mock/start/file1.txt", eventsTriggered);
        }

        [Fact]
        public void TestTraverse_WithFilter_Files()
        {
            // Arrange
            var startFolder = "/mock/start";
            // Filter: Include only ".pdf" files
            Func<string, bool> fileFilter = filePath => filePath.EndsWith("pdf");

            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object, fileFilter);

            var eventsTriggered = new List<string>();

            // Subscribe to events
            visitor.Started += (sender, e) => eventsTriggered.Add("Started");
            visitor.Finished += (sender, e) => eventsTriggered.Add("Finished");
            visitor.DirectoryFound += (sender, e) => eventsTriggered.Add($"DirectoryFound: {e.Path}");
            visitor.FileFound += (sender, e) => eventsTriggered.Add($"FileFound: {e.Path}");
            visitor.FilteredFileFound += (sender, e) => eventsTriggered.Add($"FilteredFileFound: {e.Path}");
            visitor.FilteredDirectoryFound += (sender, e) => eventsTriggered.Add($"FilteredFolderFound: {e.Path}");

            // Act
            var results = visitor.Traverse().ToList();

            // Assert events
            // veridy that the event ocurred and the three files where yielded
            Assert.Equal(3, results.Count);
            Assert.Contains("Started", eventsTriggered);
            Assert.Contains("Finished", eventsTriggered);
            Assert.Contains("FilteredFileFound: /mock/start/file2.pdf", eventsTriggered);
            Assert.Contains("FilteredFileFound: /mock/start/dir2/subdir/fileB.pdf", eventsTriggered);
            Assert.Contains("FilteredFileFound: /mock/start/dir2/subdir/fileC.pdf", eventsTriggered);
            Assert.DoesNotContain("FilteredFolderFound: /mock/start/dir2/subdir/", eventsTriggered);
        }

        [Fact]
        public void TestTraverse_WithFilter_Folders()
        {
            // Arrange
            var startFolder = "/mock/start";
            // Filter: Include only folder with dir2
            Func<string, bool> folderFilter = filePath => filePath.EndsWith("dir2");

            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object, folderFilter);

            var eventsTriggered = new List<string>();

            // Subscribe to events
            visitor.Started += (sender, e) => eventsTriggered.Add("Started");
            visitor.Finished += (sender, e) => eventsTriggered.Add("Finished");
            visitor.DirectoryFound += (sender, e) => eventsTriggered.Add($"DirectoryFound: {e.Path}");
            visitor.FileFound += (sender, e) => eventsTriggered.Add($"FileFound: {e.Path}");
            visitor.FilteredFileFound += (sender, e) => eventsTriggered.Add($"FilteredFileFound: {e.Path}");
            visitor.FilteredDirectoryFound += (sender, e) => eventsTriggered.Add($"FilteredFolderFound: {e.Path}");

            // Act
            var results = visitor.Traverse().ToList();

            // Assert events
            // No files are yielded since no filter exists
            Assert.Single(results);
            Assert.Contains("Started", eventsTriggered);
            Assert.Contains("Finished", eventsTriggered);
            Assert.Contains("FilteredFolderFound: /mock/start/dir2", eventsTriggered);

        }

        [Fact]
        public void TestTraverse_ShouldExclude_tempFilesAndPrivateFolders()
        {
            // Arrange
            var startFolder = "/mock/start";

            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object);

            var eventsTriggered = new List<string>();

            // Subscribe to events
            visitor.Started += (sender, e) => eventsTriggered.Add("Started");
            visitor.Finished += (sender, e) => eventsTriggered.Add("Finished");
            visitor.DirectoryFound += (sender, e) => eventsTriggered.Add($"DirectoryFound: {e.Path}");
            visitor.FileFound += (sender, e) => eventsTriggered.Add($"FileFound: {e.Path}");

            // Act
            var results = visitor.Traverse().ToList();

            // Assert events
            // No files are yielded since no filter exists
            Assert.Empty(results);
            Assert.Contains("Started", eventsTriggered);
            Assert.Contains("Finished", eventsTriggered);
            Assert.DoesNotContain("FilteredFolderFound: /mock/start/dir1/tempFile.temp", eventsTriggered);
            Assert.DoesNotContain("FilteredFolderFound: /mock/start/dir1/tempFile.temp", eventsTriggered);
        }
    }
}