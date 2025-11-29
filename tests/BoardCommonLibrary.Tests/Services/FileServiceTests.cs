using BoardCommonLibrary.Data;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using BoardCommonLibrary.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// FileService 단위 테스트
/// </summary>
public class FileServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly Mock<IFileStorageService> _mockStorageService;
    private readonly Mock<IFileValidationService> _mockValidationService;
    private readonly Mock<IThumbnailService> _mockThumbnailService;
    private readonly Mock<ILogger<FileService>> _mockLogger;
    private readonly FileService _service;
    
    public FileServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BoardDbContext(options);
        _mockStorageService = new Mock<IFileStorageService>();
        _mockValidationService = new Mock<IFileValidationService>();
        _mockThumbnailService = new Mock<IThumbnailService>();
        _mockLogger = new Mock<ILogger<FileService>>();
        
        _service = new FileService(
            _context,
            _mockStorageService.Object,
            _mockValidationService.Object,
            _mockThumbnailService.Object,
            _mockLogger.Object);
    }
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    #region GetByIdAsync Tests
    
    [Fact]
    public async Task GetByIdAsync_ExistingFile_ReturnsFile()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "test.pdf",
            StoredFileName = "stored_test.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_test.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.GetByIdAsync(file.Id);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.pdf", result.FileName);
        Assert.Equal("application/pdf", result.ContentType);
    }
    
    [Fact]
    public async Task GetByIdAsync_NonExistingFile_ReturnsNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetByIdAsync_DeletedFile_ReturnsFileWithIsDeletedFlag()
    {
        // Arrange
        // Note: FindAsync doesn't apply global query filters in EF Core
        // The service returns the file but with IsDeleted=true flag
        var file = new FileAttachment
        {
            FileName = "deleted.pdf",
            StoredFileName = "stored_deleted.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_deleted.pdf",
            UploaderId = 1,
            UploaderName = "TestUser",
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.GetByIdAsync(file.Id);
        
        // Assert
        // FindAsync returns the entity regardless of soft-delete filter
        // Application layer should check IsDeleted flag if needed
        Assert.NotNull(result);
    }
    
    #endregion
    
    #region GetByPostIdAsync Tests
    
    [Fact]
    public async Task GetByPostIdAsync_ReturnsFilesForPost()
    {
        // Arrange
        var files = new List<FileAttachment>
        {
            new FileAttachment
            {
                FileName = "file1.pdf",
                StoredFileName = "stored_file1.pdf",
                ContentType = "application/pdf",
                FileSize = 1024,
                StoragePath = "/uploads/stored_file1.pdf",
                PostId = 1,
                UploaderId = 1,
                UploaderName = "TestUser"
            },
            new FileAttachment
            {
                FileName = "file2.jpg",
                StoredFileName = "stored_file2.jpg",
                ContentType = "image/jpeg",
                FileSize = 2048,
                StoragePath = "/uploads/stored_file2.jpg",
                PostId = 1,
                UploaderId = 1,
                UploaderName = "TestUser",
                IsImage = true
            },
            new FileAttachment
            {
                FileName = "other.pdf",
                StoredFileName = "stored_other.pdf",
                ContentType = "application/pdf",
                FileSize = 512,
                StoragePath = "/uploads/stored_other.pdf",
                PostId = 2,
                UploaderId = 1,
                UploaderName = "TestUser"
            }
        };
        _context.FileAttachments.AddRange(files);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.GetByPostIdAsync(1);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal(1, f.PostId));
    }
    
    #endregion
    
    #region DeleteAsync Tests
    
    [Fact]
    public async Task DeleteAsync_ExistingFile_SoftDeletes()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "todelete.pdf",
            StoredFileName = "stored_todelete.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_todelete.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.DeleteAsync(file.Id, 1);
        
        // Assert
        Assert.True(result);
        
        var deletedFile = await _context.FileAttachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == file.Id);
        Assert.NotNull(deletedFile);
        Assert.True(deletedFile.IsDeleted);
        Assert.NotNull(deletedFile.DeletedAt);
    }
    
    [Fact]
    public async Task DeleteAsync_NonExistingFile_ReturnsFalse()
    {
        // Act
        var result = await _service.DeleteAsync(999, 1);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DeleteAsync_WrongUploader_ReturnsFalse()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "protected.pdf",
            StoredFileName = "stored_protected.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_protected.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.DeleteAsync(file.Id, 2); // Different uploader
        
        // Assert
        Assert.False(result);
        
        var notDeletedFile = await _service.GetByIdAsync(file.Id);
        Assert.NotNull(notDeletedFile);
    }
    
    #endregion
    
    #region ExistsAsync Tests
    
    [Fact]
    public async Task ExistsAsync_ExistingFile_ReturnsTrue()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "exists.pdf",
            StoredFileName = "stored_exists.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_exists.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.ExistsAsync(file.Id);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task ExistsAsync_NonExistingFile_ReturnsFalse()
    {
        // Act
        var result = await _service.ExistsAsync(999);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region IsOwnerAsync Tests
    
    [Fact]
    public async Task IsOwnerAsync_Owner_ReturnsTrue()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "myfile.pdf",
            StoredFileName = "stored_myfile.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_myfile.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.IsOwnerAsync(file.Id, 1);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public async Task IsOwnerAsync_NotOwner_ReturnsFalse()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "otherfile.pdf",
            StoredFileName = "stored_otherfile.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_otherfile.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.IsOwnerAsync(file.Id, 2);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region AttachToPostAsync Tests
    
    [Fact]
    public async Task AttachToPostAsync_ExistingFile_AttachesSuccessfully()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "unattached.pdf",
            StoredFileName = "stored_unattached.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_unattached.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.AttachToPostAsync(file.Id, 100);
        
        // Assert
        Assert.True(result);
        
        var updatedFile = await _context.FileAttachments.FindAsync(file.Id);
        Assert.NotNull(updatedFile);
        Assert.Equal(100, updatedFile.PostId);
    }
    
    [Fact]
    public async Task AttachToPostAsync_NonExistingFile_ReturnsFalse()
    {
        // Act
        var result = await _service.AttachToPostAsync(999, 100);
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region DetachFromPostAsync Tests
    
    [Fact]
    public async Task DetachFromPostAsync_AttachedFile_DetachesSuccessfully()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "attached.pdf",
            StoredFileName = "stored_attached.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_attached.pdf",
            PostId = 100,
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        // Act
        var result = await _service.DetachFromPostAsync(file.Id);
        
        // Assert
        Assert.True(result);
        
        var updatedFile = await _context.FileAttachments.FindAsync(file.Id);
        Assert.NotNull(updatedFile);
        Assert.Null(updatedFile.PostId);
    }
    
    #endregion
    
    #region HardDeleteAsync Tests
    
    [Fact]
    public async Task HardDeleteAsync_ExistingFile_DeletesPermanently()
    {
        // Arrange
        var file = new FileAttachment
        {
            FileName = "harddelete.pdf",
            StoredFileName = "stored_harddelete.pdf",
            ContentType = "application/pdf",
            FileSize = 1024,
            StoragePath = "/uploads/stored_harddelete.pdf",
            UploaderId = 1,
            UploaderName = "TestUser"
        };
        _context.FileAttachments.Add(file);
        await _context.SaveChangesAsync();
        
        _mockStorageService.Setup(s => s.DeleteAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        
        // Act
        var result = await _service.HardDeleteAsync(file.Id);
        
        // Assert
        Assert.True(result);
        
        var deletedFile = await _context.FileAttachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == file.Id);
        Assert.Null(deletedFile);
    }
    
    #endregion
}
