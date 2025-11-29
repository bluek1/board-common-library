using BoardCommonLibrary.Interfaces;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 로컬 파일 시스템 저장소 서비스 구현
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalFileStorageService> _logger;
    
    /// <summary>
    /// 로컬 파일 저장소 서비스 생성자
    /// </summary>
    /// <param name="basePath">파일 저장 기본 경로</param>
    /// <param name="baseUrl">파일 URL 기본 경로</param>
    /// <param name="logger">로거</param>
    public LocalFileStorageService(string basePath, string baseUrl, ILogger<LocalFileStorageService> logger)
    {
        _basePath = basePath;
        _baseUrl = baseUrl.TrimEnd('/');
        _logger = logger;
        
        // 기본 디렉토리 생성
        if (!Directory.Exists(_basePath))
        {
            Directory.CreateDirectory(_basePath);
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> SaveAsync(Stream stream, string fileName, string contentType, string? subFolder = null)
    {
        var folder = string.IsNullOrEmpty(subFolder) ? _basePath : Path.Combine(_basePath, subFolder);
        
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        var filePath = Path.Combine(folder, fileName);
        var relativePath = string.IsNullOrEmpty(subFolder) ? fileName : $"{subFolder}/{fileName}";
        
        try
        {
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream);
            
            _logger.LogInformation("파일 저장 완료: {FilePath}", relativePath);
            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 저장 실패: {FilePath}", filePath);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<Stream?> ReadAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
        
        if (!File.Exists(fullPath))
        {
            _logger.LogWarning("파일을 찾을 수 없음: {FilePath}", fullPath);
            return null;
        }
        
        try
        {
            // 메모리 스트림으로 복사하여 반환 (파일 잠금 방지)
            var memoryStream = new MemoryStream();
            using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 읽기 실패: {FilePath}", fullPath);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
        
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("파일 삭제 완료: {FilePath}", filePath);
                return Task.FromResult(true);
            }
            
            _logger.LogWarning("삭제할 파일을 찾을 수 없음: {FilePath}", fullPath);
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 삭제 실패: {FilePath}", fullPath);
            return Task.FromResult(false);
        }
    }
    
    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
        return Task.FromResult(File.Exists(fullPath));
    }
    
    /// <inheritdoc/>
    public async Task<bool> CopyAsync(string sourcePath, string destinationPath)
    {
        var sourceFullPath = Path.Combine(_basePath, sourcePath.Replace('/', Path.DirectorySeparatorChar));
        var destFullPath = Path.Combine(_basePath, destinationPath.Replace('/', Path.DirectorySeparatorChar));
        
        try
        {
            var destDir = Path.GetDirectoryName(destFullPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            
            using var sourceStream = new FileStream(sourceFullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var destStream = new FileStream(destFullPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await sourceStream.CopyToAsync(destStream);
            
            _logger.LogInformation("파일 복사 완료: {Source} -> {Dest}", sourcePath, destinationPath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 복사 실패: {Source} -> {Dest}", sourcePath, destinationPath);
            return false;
        }
    }
    
    /// <inheritdoc/>
    public Task<bool> MoveAsync(string sourcePath, string destinationPath)
    {
        var sourceFullPath = Path.Combine(_basePath, sourcePath.Replace('/', Path.DirectorySeparatorChar));
        var destFullPath = Path.Combine(_basePath, destinationPath.Replace('/', Path.DirectorySeparatorChar));
        
        try
        {
            var destDir = Path.GetDirectoryName(destFullPath);
            if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            
            File.Move(sourceFullPath, destFullPath);
            _logger.LogInformation("파일 이동 완료: {Source} -> {Dest}", sourcePath, destinationPath);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "파일 이동 실패: {Source} -> {Dest}", sourcePath, destinationPath);
            return Task.FromResult(false);
        }
    }
    
    /// <inheritdoc/>
    public string GetUrl(string filePath)
    {
        return $"{_baseUrl}/{filePath.Replace(Path.DirectorySeparatorChar, '/')}";
    }
    
    /// <inheritdoc/>
    public Task<long> GetFileSizeAsync(string filePath)
    {
        var fullPath = Path.Combine(_basePath, filePath.Replace('/', Path.DirectorySeparatorChar));
        
        if (File.Exists(fullPath))
        {
            var fileInfo = new FileInfo(fullPath);
            return Task.FromResult(fileInfo.Length);
        }
        
        return Task.FromResult(0L);
    }
    
    /// <inheritdoc/>
    public Task<long> GetUsedStorageAsync()
    {
        if (!Directory.Exists(_basePath))
        {
            return Task.FromResult(0L);
        }
        
        var directoryInfo = new DirectoryInfo(_basePath);
        var totalSize = directoryInfo.EnumerateFiles("*", SearchOption.AllDirectories)
            .Sum(file => file.Length);
        
        return Task.FromResult(totalSize);
    }
    
    /// <inheritdoc/>
    public Task<IEnumerable<string>> ListFilesAsync(string folderPath)
    {
        var fullPath = Path.Combine(_basePath, folderPath.Replace('/', Path.DirectorySeparatorChar));
        
        if (!Directory.Exists(fullPath))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
        
        var files = Directory.GetFiles(fullPath, "*", SearchOption.TopDirectoryOnly)
            .Select(f => Path.GetRelativePath(_basePath, f).Replace(Path.DirectorySeparatorChar, '/'));
        
        return Task.FromResult(files);
    }
    
    /// <inheritdoc/>
    public Task<bool> DeleteFolderAsync(string folderPath)
    {
        var fullPath = Path.Combine(_basePath, folderPath.Replace('/', Path.DirectorySeparatorChar));
        
        try
        {
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
                _logger.LogInformation("폴더 삭제 완료: {FolderPath}", folderPath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "폴더 삭제 실패: {FolderPath}", folderPath);
            return Task.FromResult(false);
        }
    }
}
