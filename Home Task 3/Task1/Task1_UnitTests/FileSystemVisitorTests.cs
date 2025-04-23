using Task1_FileSystemVisitorApp;
using Moq;
using System.IO.Abstractions;

namespace Task1_UnitTests
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
                { "/mock/start", new List<string> { "/mock/start/dir1", "/mock/start/dir2" } },
                { "/mock/start/dir1", new List<string>() },
                { "/mock/start/dir2", new List<string> { "/mock/start/dir2/subdir" } },
                { "/mock/start/dir2/subdir", new List<string>() },
            };

            files = new Dictionary<string, List<string>>
            {
                { "/mock/start", new List<string> { "/mock/start/file1.txt", "/mock/start/file2.pdf" } },
                { "/mock/start/dir1", new List<string> { "/mock/start/dir1/fileA.txt" } },
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
        public void Traverse_ShouldYieldAllFilesAndDirectories_NoFilterSpecified()
        {
            // Arrange
            var startFolder = "/mock/start";

            // System under test: FileSystemVisitor
            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object);

            var expected = new[]
            {
                "/mock/start",
                "/mock/start/file1.txt",
                "/mock/start/file2.pdf",
                "/mock/start/dir1",
                "/mock/start/dir1/fileA.txt",
                "/mock/start/dir2",
                "/mock/start/dir2/subdir",
                "/mock/start/dir2/subdir/fileB.pdf",
                "/mock/start/dir2/subdir/fileC.pdf"
            };

            // Act
            var result = visitor.Traverse().ToList();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Traverse_WithFileFilter_ShouldYield_PDF_Files()
        {
            // Arrange
            var startFolder = "/mock/start";

            // Filter: Include only ".pdf" files
            Func<string, bool> fileFilter = filePath => filePath.EndsWith("pdf");

            // System under test: FileSystemVisitor
            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object, fileFilter);

            var expected = new[]
            {
                "/mock/start/file2.pdf",
                "/mock/start/dir2/subdir/fileB.pdf",
                "/mock/start/dir2/subdir/fileC.pdf"
            };

            // Act
            var result = visitor.Traverse().ToList();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Traverse_WithFolderFilter_ShouldYieldFolders()
        {
            // Arrange
            var startFolder = "/mock/start";

            // Filter: Include only ".txt" files
            string filteredFolder = "dir2";
            Func<string, bool> folderFilter = filePath => filePath.EndsWith(filteredFolder);

            // System under test: FileSystemVisitor
            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object, folderFilter);

            var expected = new[]
            {
                "/mock/start/dir2",
            };

            // Act
            var result = visitor.Traverse().ToList();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Traverse_WithFileFilter_NoMatch()
        {
            // Arrange
            var startFolder = "/mock/start";

            string filteredFile = "exe";
            Func<string, bool> fileFilter = filePath => filePath.EndsWith(filteredFile);

            // System under test: FileSystemVisitor
            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object, fileFilter);

            // Act
            var result = visitor.Traverse().ToList();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void Traverse_WithFileFolder_NoMatch()
        {
            // Arrange
            var startFolder = "/mock/start";

            // Filter: Include only ".txt" files
            string filteredFolder = "NewFolder";
            Func<string, bool> folderFilter = filePath => filePath.EndsWith(filteredFolder);

            // System under test: FileSystemVisitor
            var visitor = new FileSystemVisitor(startFolder, mockFileSystem.Object, folderFilter);

            // Act
            var result = visitor.Traverse().ToList();

            // Assert
            Assert.Empty(result);
        }
    }
}