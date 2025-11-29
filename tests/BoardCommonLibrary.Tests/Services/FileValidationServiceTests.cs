using BoardCommonLibrary.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// FileValidationService 단위 테스트
/// </summary>
public class FileValidationServiceTests
{
    private readonly Mock<ILogger<FileValidationService>> _mockLogger;
    private readonly FileValidationService _service;
    
    public FileValidationServiceTests()
    {
        _mockLogger = new Mock<ILogger<FileValidationService>>();
        _service = new FileValidationService(_mockLogger.Object);
    }
    
    #region ValidateAsync Tests
    
    [Fact]
    public async Task ValidateAsync_ValidImage_ReturnsValid()
    {
        // Arrange
        var file = CreateMockJpegFile("test.jpg", 1024 * 1024); // 1MB
        
        // Act
        var result = await _service.ValidateAsync(file);
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
    
    [Fact]
    public async Task ValidateAsync_ValidPdf_ReturnsValid()
    {
        // Arrange
        var file = CreateMockPdfFile("document.pdf", 5 * 1024 * 1024); // 5MB
        
        // Act
        var result = await _service.ValidateAsync(file);
        
        // Assert
        Assert.True(result.IsValid);
    }
    
    [Fact]
    public async Task ValidateAsync_ExceedsMaxSize_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockJpegFile("large.jpg", 20 * 1024 * 1024); // 20MB
        
        // Act
        var result = await _service.ValidateAsync(file);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("크기"));
    }
    
    [Fact]
    public async Task ValidateAsync_DisallowedExtension_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFormFile("script.exe", "application/octet-stream", 1024);
        
        // Act
        var result = await _service.ValidateAsync(file);
        
        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("파일 형식") || e.Contains("확장자"));
    }
    
    [Fact]
    public async Task ValidateAsync_EmptyFile_ReturnsInvalid()
    {
        // Arrange
        var file = CreateMockFormFile("empty.pdf", "application/pdf", 0);
        
        // Act
        var result = await _service.ValidateAsync(file);
        
        // Assert
        Assert.False(result.IsValid);
    }
    
    #endregion
    
    #region ValidateFileSize Tests
    
    [Fact]
    public void ValidateFileSize_WithinLimit_ReturnsTrue()
    {
        // Arrange
        var file = CreateMockFormFile("test.jpg", "image/jpeg", 5 * 1024 * 1024); // 5MB
        
        // Act
        var result = _service.ValidateFileSize(file);
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateFileSize_ExceedsLimit_ReturnsFalse()
    {
        // Arrange
        var file = CreateMockFormFile("large.jpg", "image/jpeg", 20 * 1024 * 1024); // 20MB
        
        // Act
        var result = _service.ValidateFileSize(file);
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateFileSize_CustomLimit_ReturnsExpected()
    {
        // Arrange
        var file = CreateMockFormFile("test.jpg", "image/jpeg", 2 * 1024 * 1024); // 2MB
        
        // Act
        var result = _service.ValidateFileSize(file, 1 * 1024 * 1024); // 1MB limit
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region ValidateExtension Tests
    
    [Theory]
    [InlineData("test.jpg", true)]
    [InlineData("test.jpeg", true)]
    [InlineData("test.png", true)]
    [InlineData("test.gif", true)]
    [InlineData("test.pdf", true)]
    [InlineData("test.doc", true)]
    [InlineData("test.docx", true)]
    [InlineData("test.exe", false)]
    [InlineData("test.bat", false)]
    [InlineData("test.sh", false)]
    [InlineData("test.php", false)]
    public void ValidateExtension_VariousExtensions_ReturnsExpected(string fileName, bool expected)
    {
        // Act
        var result = _service.ValidateExtension(fileName);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void ValidateExtension_CaseInsensitive_ReturnsTrue()
    {
        // Act & Assert
        Assert.True(_service.ValidateExtension("test.JPG"));
        Assert.True(_service.ValidateExtension("test.Pdf"));
        Assert.True(_service.ValidateExtension("test.PNG"));
    }
    
    #endregion
    
    #region ValidateContentType Tests
    
    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/gif", true)]
    [InlineData("application/pdf", true)]
    [InlineData("application/octet-stream", false)]
    [InlineData("application/x-msdownload", false)]
    public void ValidateContentType_VariousTypes_ReturnsExpected(string contentType, bool expected)
    {
        // Act
        var result = _service.ValidateContentType(contentType);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    #endregion
    
    #region ValidateFileName Tests
    
    [Fact]
    public void ValidateFileName_ValidFileName_ReturnsTrue()
    {
        // Act
        var result = _service.ValidateFileName("document.pdf");
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateFileName_ContainsInvalidChars_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateFileName("file<name>.pdf");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateFileName_ContainsPathTraversal_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateFileName("../../../etc/passwd");
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateFileName_EmptyString_ReturnsFalse()
    {
        // Act
        var result = _service.ValidateFileName("");
        
        // Assert
        Assert.False(result);
    }
    
    #endregion
    
    #region IsImageFile Tests
    
    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/gif", true)]
    [InlineData("image/webp", true)]
    [InlineData("image/bmp", true)]
    [InlineData("application/pdf", false)]
    [InlineData("text/plain", false)]
    [InlineData("application/octet-stream", false)]
    public void IsImageFile_VariousContentTypes_ReturnsExpected(string contentType, bool expected)
    {
        // Act
        var result = _service.IsImageFile(contentType);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    #endregion
    
    #region GenerateSafeFileName Tests
    
    [Fact]
    public void GenerateSafeFileName_ValidFileName_ReturnsSame()
    {
        // Act
        var result = _service.GenerateSafeFileName("document.pdf");
        
        // Assert
        Assert.Equal("document.pdf", result);
    }
    
    [Fact]
    public void GenerateSafeFileName_ContainsSpecialChars_RemovesThem()
    {
        // Act
        var result = _service.GenerateSafeFileName("file<name>.pdf");
        
        // Assert
        Assert.DoesNotContain("<", result);
        Assert.DoesNotContain(">", result);
    }
    
    [Fact]
    public void GenerateSafeFileName_ContainsPathSeparators_RemovesThem()
    {
        // Act
        var result = _service.GenerateSafeFileName("path/to/file.pdf");
        
        // Assert
        Assert.DoesNotContain("/", result);
    }
    
    #endregion
    
    #region GenerateUniqueFileName Tests
    
    [Fact]
    public void GenerateUniqueFileName_PreservesExtension()
    {
        // Act
        var result = _service.GenerateUniqueFileName("test.pdf");
        
        // Assert
        Assert.EndsWith(".pdf", result);
    }
    
    [Fact]
    public void GenerateUniqueFileName_GeneratesUnique()
    {
        // Act
        var result1 = _service.GenerateUniqueFileName("test.pdf");
        var result2 = _service.GenerateUniqueFileName("test.pdf");
        
        // Assert
        Assert.NotEqual(result1, result2);
    }
    
    [Fact]
    public void GenerateUniqueFileName_NoExtension_ReturnsValidName()
    {
        // Act
        var result = _service.GenerateUniqueFileName("noext");
        
        // Assert
        Assert.NotEmpty(result);
    }
    
    #endregion
    
    #region GetAllowedExtensions / GetAllowedContentTypes / GetMaxFileSize Tests
    
    [Fact]
    public void GetAllowedExtensions_ReturnsDefaultList()
    {
        // Act
        var result = _service.GetAllowedExtensions();
        
        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(".jpg", result);
        Assert.Contains(".pdf", result);
    }
    
    [Fact]
    public void GetAllowedContentTypes_ReturnsDefaultList()
    {
        // Act
        var result = _service.GetAllowedContentTypes();
        
        // Assert
        Assert.NotEmpty(result);
        Assert.Contains("image/jpeg", result);
        Assert.Contains("application/pdf", result);
    }
    
    [Fact]
    public void GetMaxFileSize_ReturnsDefault()
    {
        // Act
        var result = _service.GetMaxFileSize();
        
        // Assert
        Assert.Equal(10 * 1024 * 1024, result); // 10MB default
    }
    
    #endregion
    
    #region Helper Methods
    
    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var content = new byte[length > 0 ? Math.Min((int)length, 1024) : 0];
        var stream = new MemoryStream(content);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((s, ct) => stream.CopyToAsync(s, ct));
        return mockFile.Object;
    }
    
    private static IFormFile CreateMockJpegFile(string fileName, long length)
    {
        // JPEG 시그니처
        var content = new byte[Math.Max(10, Math.Min((int)length, 1024))];
        content[0] = 0xFF;
        content[1] = 0xD8;
        content[2] = 0xFF;
        
        var stream = new MemoryStream(content);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(content));
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((s, ct) => new MemoryStream(content).CopyToAsync(s, ct));
        return mockFile.Object;
    }
    
    private static IFormFile CreateMockPdfFile(string fileName, long length)
    {
        // PDF 시그니처
        var content = new byte[Math.Max(10, Math.Min((int)length, 1024))];
        content[0] = 0x25; // %
        content[1] = 0x50; // P
        content[2] = 0x44; // D
        content[3] = 0x46; // F
        
        var stream = new MemoryStream(content);
        
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns("application/pdf");
        mockFile.Setup(f => f.Length).Returns(length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(content));
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((s, ct) => new MemoryStream(content).CopyToAsync(s, ct));
        return mockFile.Object;
    }
    
    #endregion
}
