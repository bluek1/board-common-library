using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 파일 관리 서비스 구현
/// </summary>
public class FileService : IFileService
{
    private readonly BoardDbContext _context;
    private readonly IFileStorageService _storageService;
    private readonly IFileValidationService _validationService;
    private readonly IThumbnailService _thumbnailService;
    private readonly ILogger<FileService> _logger;
    private readonly string _fileUrlBase;
    
    /// <summary>
    /// 파일 서비스 생성자
    /// </summary>
    public FileService(
        BoardDbContext context,
        IFileStorageService storageService,
        IFileValidationService validationService,
        IThumbnailService thumbnailService,
        ILogger<FileService> logger,
        string fileUrlBase = "/api/files")
    {
        _context = context;
        _storageService = storageService;
        _validationService = validationService;
        _thumbnailService = thumbnailService;
        _logger = logger;
        _fileUrlBase = fileUrlBase.TrimEnd('/');
    }
    
    /// <inheritdoc/>
    public async Task<FileInfoResponse> UploadAsync(IFormFile file, long uploaderId, string uploaderName, long? postId = null)
    {
        // 파일 검증
        var validationResult = await _validationService.ValidateAsync(file);
        if (!validationResult.IsValid)
        {
            throw new InvalidOperationException(string.Join("; ", validationResult.Errors));
        }
        
        // 고유 파일명 생성
        var storedFileName = _validationService.GenerateUniqueFileName(file.FileName);
        
        // 날짜 기반 폴더 구조 (yyyy/MM)
        var subFolder = DateTime.UtcNow.ToString("yyyy/MM");
        
        // 파일 저장
        string storagePath;
        using (var stream = file.OpenReadStream())
        {
            storagePath = await _storageService.SaveAsync(stream, storedFileName, file.ContentType, subFolder);
        }
        
        // 이미지인 경우 썸네일 생성
        string? thumbnailPath = null;
        int? width = null;
        int? height = null;
        
        if (validationResult.IsImage == true && _thumbnailService.CanProcess(file.ContentType))
        {
            try
            {
                using var imageStream = file.OpenReadStream();
                var dimensions = await _thumbnailService.GetImageDimensionsAsync(imageStream);
                width = dimensions.Width;
                height = dimensions.Height;
                
                imageStream.Position = 0;
                var (thumbWidth, thumbHeight) = _thumbnailService.GetDefaultThumbnailSize();
                thumbnailPath = await _thumbnailService.GenerateAndSaveAsync(imageStream, storagePath, thumbWidth, thumbHeight);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "썸네일 생성 실패: {FileName}", file.FileName);
                // 썸네일 생성 실패는 업로드를 중단하지 않음
            }
        }
        
        // 데이터베이스에 파일 정보 저장
        var fileAttachment = new FileAttachment
        {
            FileName = _validationService.GenerateSafeFileName(file.FileName),
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            StoragePath = storagePath,
            ThumbnailPath = thumbnailPath,
            PostId = postId,
            UploaderId = uploaderId,
            UploaderName = uploaderName,
            IsImage = validationResult.IsImage ?? false,
            Width = width,
            Height = height
        };
        
        _context.FileAttachments.Add(fileAttachment);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("파일 업로드 완료: Id={Id}, FileName={FileName}", fileAttachment.Id, file.FileName);
        
        return MapToResponse(fileAttachment);
    }
    
    /// <inheritdoc/>
    public async Task<MultipleFileUploadResponse> UploadMultipleAsync(List<IFormFile> files, long uploaderId, string uploaderName, long? postId = null)
    {
        var response = new MultipleFileUploadResponse
        {
            TotalCount = files.Count
        };
        
        foreach (var file in files)
        {
            try
            {
                var uploaded = await UploadAsync(file, uploaderId, uploaderName, postId);
                response.Files.Add(uploaded);
                response.SuccessCount++;
            }
            catch (Exception ex)
            {
                response.FailedCount++;
                response.Errors.Add(new FileUploadError
                {
                    FileName = file.FileName,
                    ErrorCode = "UPLOAD_FAILED",
                    ErrorMessage = ex.Message
                });
                _logger.LogWarning(ex, "파일 업로드 실패: {FileName}", file.FileName);
            }
        }
        
        response.Message = $"{response.SuccessCount}개 파일 업로드 완료" +
            (response.FailedCount > 0 ? $", {response.FailedCount}개 실패" : "");
        
        return response;
    }
    
    /// <inheritdoc/>
    public async Task<FileInfoResponse?> GetByIdAsync(long id)
    {
        var file = await _context.FileAttachments.FindAsync(id);
        return file == null ? null : MapToResponse(file);
    }
    
    /// <inheritdoc/>
    public async Task<FileDownloadResponse?> DownloadAsync(long id)
    {
        var file = await _context.FileAttachments.FindAsync(id);
        if (file == null)
            return null;
        
        var stream = await _storageService.ReadAsync(file.StoragePath);
        if (stream == null)
            return null;
        
        // 다운로드 횟수 증가
        file.DownloadCount++;
        await _context.SaveChangesAsync();
        
        return new FileDownloadResponse
        {
            FileStream = stream,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize
        };
    }
    
    /// <inheritdoc/>
    public async Task<FileDownloadResponse?> GetThumbnailAsync(long id)
    {
        var file = await _context.FileAttachments.FindAsync(id);
        if (file == null || string.IsNullOrEmpty(file.ThumbnailPath))
            return null;
        
        var stream = await _storageService.ReadAsync(file.ThumbnailPath);
        if (stream == null)
            return null;
        
        return new FileDownloadResponse
        {
            FileStream = stream,
            FileName = $"thumb_{file.FileName}",
            ContentType = "image/jpeg",
            FileSize = stream.Length
        };
    }
    
    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(long id, long userId)
    {
        var file = await _context.FileAttachments.FindAsync(id);
        if (file == null)
            return false;
        
        // 소유자 확인
        if (file.UploaderId != userId)
        {
            _logger.LogWarning("파일 삭제 권한 없음: FileId={Id}, UserId={UserId}", id, userId);
            return false;
        }
        
        // 소프트 삭제
        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("파일 삭제됨 (소프트): Id={Id}", id);
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> HardDeleteAsync(long id)
    {
        var file = await _context.FileAttachments
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == id);
            
        if (file == null)
            return false;
        
        // 물리적 파일 삭제
        await _storageService.DeleteAsync(file.StoragePath);
        if (!string.IsNullOrEmpty(file.ThumbnailPath))
        {
            await _storageService.DeleteAsync(file.ThumbnailPath);
        }
        
        // 데이터베이스에서 삭제
        _context.FileAttachments.Remove(file);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("파일 영구 삭제됨: Id={Id}", id);
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<List<FileInfoResponse>> GetByPostIdAsync(long postId)
    {
        var files = await _context.FileAttachments
            .Where(f => f.PostId == postId)
            .OrderBy(f => f.CreatedAt)
            .ToListAsync();
        
        return files.Select(MapToResponse).ToList();
    }
    
    /// <inheritdoc/>
    public async Task<PagedResponse<FileInfoResponse>> GetByUploaderAsync(long uploaderId, FileQueryParameters parameters)
    {
        var query = _context.FileAttachments
            .Where(f => f.UploaderId == uploaderId);
        
        return await GetPagedFilesAsync(query, parameters);
    }
    
    /// <inheritdoc/>
    public async Task<PagedResponse<FileInfoResponse>> GetAllAsync(FileQueryParameters parameters)
    {
        var query = _context.FileAttachments.AsQueryable();
        
        // 필터 적용
        if (parameters.PostId.HasValue)
            query = query.Where(f => f.PostId == parameters.PostId);
        
        if (parameters.UploaderId.HasValue)
            query = query.Where(f => f.UploaderId == parameters.UploaderId);
        
        if (!string.IsNullOrEmpty(parameters.ContentType))
        {
            if (parameters.ContentType.EndsWith("/*"))
            {
                var prefix = parameters.ContentType[..^1];
                query = query.Where(f => f.ContentType.StartsWith(prefix));
            }
            else
            {
                query = query.Where(f => f.ContentType == parameters.ContentType);
            }
        }
        
        if (parameters.IsImage.HasValue)
            query = query.Where(f => f.IsImage == parameters.IsImage);
        
        return await GetPagedFilesAsync(query, parameters);
    }
    
    /// <inheritdoc/>
    public async Task<bool> AttachToPostAsync(long fileId, long postId)
    {
        var file = await _context.FileAttachments.FindAsync(fileId);
        if (file == null)
            return false;
        
        file.PostId = postId;
        file.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("파일을 게시물에 연결: FileId={FileId}, PostId={PostId}", fileId, postId);
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> DetachFromPostAsync(long fileId)
    {
        var file = await _context.FileAttachments.FindAsync(fileId);
        if (file == null)
            return false;
        
        file.PostId = null;
        file.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("파일을 게시물에서 분리: FileId={FileId}", fileId);
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(long id)
    {
        return await _context.FileAttachments.AnyAsync(f => f.Id == id);
    }
    
    /// <inheritdoc/>
    public async Task<bool> IsOwnerAsync(long id, long userId)
    {
        return await _context.FileAttachments.AnyAsync(f => f.Id == id && f.UploaderId == userId);
    }
    
    /// <summary>
    /// 페이징된 파일 목록 조회
    /// </summary>
    private async Task<PagedResponse<FileInfoResponse>> GetPagedFilesAsync(IQueryable<FileAttachment> query, FileQueryParameters parameters)
    {
        // 정렬 적용
        query = parameters.Sort.ToLowerInvariant() switch
        {
            "filename" => parameters.Order.ToLowerInvariant() == "asc"
                ? query.OrderBy(f => f.FileName)
                : query.OrderByDescending(f => f.FileName),
            "filesize" => parameters.Order.ToLowerInvariant() == "asc"
                ? query.OrderBy(f => f.FileSize)
                : query.OrderByDescending(f => f.FileSize),
            _ => parameters.Order.ToLowerInvariant() == "asc"
                ? query.OrderBy(f => f.CreatedAt)
                : query.OrderByDescending(f => f.CreatedAt)
        };
        
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)parameters.PageSize);
        
        var files = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();
        
        return new PagedResponse<FileInfoResponse>
        {
            Data = files.Select(MapToResponse).ToList(),
            Meta = new PagedMetaData
            {
                TotalCount = totalCount,
                TotalPages = totalPages,
                Page = parameters.Page,
                PageSize = parameters.PageSize
            }
        };
    }
    
    /// <summary>
    /// 엔티티를 응답 DTO로 변환
    /// </summary>
    private FileInfoResponse MapToResponse(FileAttachment file)
    {
        return new FileInfoResponse
        {
            Id = file.Id,
            FileName = file.FileName,
            StoredFileName = file.StoredFileName,
            ContentType = file.ContentType,
            FileSize = file.FileSize,
            FileSizeFormatted = FormatFileSize(file.FileSize),
            PostId = file.PostId,
            UploaderId = file.UploaderId,
            UploaderName = file.UploaderName,
            DownloadCount = file.DownloadCount,
            IsImage = file.IsImage,
            Width = file.Width,
            Height = file.Height,
            ThumbnailUrl = string.IsNullOrEmpty(file.ThumbnailPath) ? null : $"{_fileUrlBase}/{file.Id}/thumbnail",
            DownloadUrl = $"{_fileUrlBase}/{file.Id}",
            CreatedAt = file.CreatedAt,
            UpdatedAt = file.UpdatedAt
        };
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
