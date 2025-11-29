using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 파일 검증 서비스 구현
/// </summary>
public class FileValidationService : IFileValidationService
{
    private readonly ILogger<FileValidationService> _logger;
    private readonly long _maxFileSize;
    private readonly List<string> _allowedExtensions;
    private readonly List<string> _allowedContentTypes;
    
    // 파일 시그니처 (매직 넘버) 정의
    private static readonly Dictionary<string, byte[][]> FileSignatures = new()
    {
        { ".jpg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new[] { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".gif", new[] { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
        { ".bmp", new[] { new byte[] { 0x42, 0x4D } } },
        { ".webp", new[] { new byte[] { 0x52, 0x49, 0x46, 0x46 } } },
        { ".pdf", new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
        { ".doc", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".docx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".xls", new[] { new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 } } },
        { ".xlsx", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".zip", new[] { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } },
        { ".txt", new byte[][] { } }, // 텍스트 파일은 시그니처 없음
    };
    
    // 위험한 파일명 패턴
    private static readonly HashSet<char> InvalidFileNameChars = new(Path.GetInvalidFileNameChars());
    
    /// <summary>
    /// 파일 검증 서비스 생성자
    /// </summary>
    /// <param name="logger">로거</param>
    /// <param name="maxFileSize">최대 파일 크기 (bytes)</param>
    /// <param name="allowedExtensions">허용된 확장자 목록</param>
    /// <param name="allowedContentTypes">허용된 MIME 타입 목록</param>
    public FileValidationService(
        ILogger<FileValidationService> logger,
        long maxFileSize = 10 * 1024 * 1024, // 기본 10MB
        List<string>? allowedExtensions = null,
        List<string>? allowedContentTypes = null)
    {
        _logger = logger;
        _maxFileSize = maxFileSize;
        
        _allowedExtensions = allowedExtensions ?? new List<string>
        {
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
            ".pdf", ".doc", ".docx", ".xls", ".xlsx",
            ".txt", ".zip"
        };
        
        _allowedContentTypes = allowedContentTypes ?? new List<string>
        {
            "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp",
            "application/pdf",
            "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain",
            "application/zip", "application/x-zip-compressed"
        };
    }
    
    /// <inheritdoc/>
    public async Task<FileValidationResult> ValidateAsync(IFormFile file)
    {
        var result = new FileValidationResult { IsValid = true };
        
        // 파일명 검증
        if (!ValidateFileName(file.FileName))
        {
            result.IsValid = false;
            result.Errors.Add("파일명에 허용되지 않는 문자가 포함되어 있습니다.");
        }
        
        // 파일 크기 검증
        if (!ValidateFileSize(file))
        {
            result.IsValid = false;
            result.Errors.Add($"파일 크기가 {FormatFileSize(_maxFileSize)}를 초과합니다.");
        }
        
        // 확장자 검증
        if (!ValidateExtension(file.FileName))
        {
            result.IsValid = false;
            result.Errors.Add($"허용되지 않는 파일 형식입니다. 허용된 형식: {string.Join(", ", _allowedExtensions)}");
        }
        
        // MIME 타입 검증
        if (!ValidateContentType(file.ContentType))
        {
            result.IsValid = false;
            result.Errors.Add($"허용되지 않는 MIME 타입입니다: {file.ContentType}");
        }
        
        // 파일 시그니처 검증
        var signatureResult = await ValidateFileSignatureAsync(file);
        if (!signatureResult.IsValid)
        {
            result.IsValid = false;
            result.Errors.AddRange(signatureResult.Errors);
        }
        
        result.DetectedContentType = signatureResult.DetectedContentType ?? file.ContentType;
        result.IsImage = IsImageFile(result.DetectedContentType ?? file.ContentType);
        
        return result;
    }
    
    /// <inheritdoc/>
    public bool ValidateFileSize(IFormFile file, long? maxSize = null)
    {
        var limit = maxSize ?? _maxFileSize;
        return file.Length <= limit;
    }
    
    /// <inheritdoc/>
    public bool ValidateExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
    
    /// <inheritdoc/>
    public async Task<FileValidationResult> ValidateFileSignatureAsync(IFormFile file)
    {
        var result = new FileValidationResult { IsValid = true };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        // 시그니처 정의가 없는 확장자는 통과
        if (!FileSignatures.TryGetValue(extension, out var signatures) || signatures.Length == 0)
        {
            return result;
        }
        
        // 가장 긴 시그니처 길이만큼 읽기
        var maxSignatureLength = signatures.Max(s => s.Length);
        var headerBytes = new byte[maxSignatureLength];
        
        using var stream = file.OpenReadStream();
        var bytesRead = await stream.ReadAsync(headerBytes, 0, maxSignatureLength);
        
        // 시그니처 매칭 확인
        var isMatch = signatures.Any(signature =>
        {
            if (bytesRead < signature.Length) return false;
            return signature.SequenceEqual(headerBytes.Take(signature.Length));
        });
        
        if (!isMatch)
        {
            result.IsValid = false;
            result.Errors.Add("파일 내용이 확장자와 일치하지 않습니다.");
            _logger.LogWarning("파일 시그니처 불일치: {FileName}", file.FileName);
        }
        
        return result;
    }
    
    /// <inheritdoc/>
    public bool ValidateContentType(string contentType)
    {
        return _allowedContentTypes.Contains(contentType.ToLowerInvariant());
    }
    
    /// <inheritdoc/>
    public bool ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;
        
        // 위험한 문자 포함 여부
        if (fileName.Any(c => InvalidFileNameChars.Contains(c)))
            return false;
        
        // 상위 디렉토리 접근 시도
        if (fileName.Contains(".."))
            return false;
        
        // 경로 구분자 포함
        if (fileName.Contains('/') || fileName.Contains('\\'))
            return false;
        
        // 숨김 파일 (Unix)
        if (fileName.StartsWith('.'))
            return false;
        
        return true;
    }
    
    /// <inheritdoc/>
    public bool IsImageFile(string contentType)
    {
        return contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
    
    /// <inheritdoc/>
    public IReadOnlyList<string> GetAllowedExtensions()
    {
        return _allowedExtensions.AsReadOnly();
    }
    
    /// <inheritdoc/>
    public IReadOnlyList<string> GetAllowedContentTypes()
    {
        return _allowedContentTypes.AsReadOnly();
    }
    
    /// <inheritdoc/>
    public long GetMaxFileSize()
    {
        return _maxFileSize;
    }
    
    /// <inheritdoc/>
    public string GenerateSafeFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        
        // 위험한 문자 제거
        var safeChars = nameWithoutExtension
            .Where(c => !InvalidFileNameChars.Contains(c) && c != '.')
            .ToArray();
        
        var safeName = new string(safeChars);
        
        // 빈 이름이면 기본값 사용
        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "file";
        }
        
        return safeName + extension.ToLowerInvariant();
    }
    
    /// <inheritdoc/>
    public string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        return $"{timestamp}_{uniqueId}{extension}";
    }
    
    /// <summary>
    /// 파일 크기를 사람이 읽기 쉬운 형식으로 변환
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }
}
