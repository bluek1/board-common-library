using BoardCommonLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace BoardCommonLibrary.Services;

/// <summary>
/// 썸네일 생성 서비스 구현 (ImageSharp 사용)
/// </summary>
public class ThumbnailService : IThumbnailService
{
    private readonly IFileStorageService _storageService;
    private readonly ILogger<ThumbnailService> _logger;
    private readonly int _defaultWidth;
    private readonly int _defaultHeight;
    
    private static readonly HashSet<string> SupportedImageFormats = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/webp"
    };
    
    /// <summary>
    /// 썸네일 서비스 생성자
    /// </summary>
    /// <param name="storageService">파일 저장소 서비스</param>
    /// <param name="logger">로거</param>
    /// <param name="defaultWidth">기본 썸네일 너비</param>
    /// <param name="defaultHeight">기본 썸네일 높이</param>
    public ThumbnailService(
        IFileStorageService storageService,
        ILogger<ThumbnailService> logger,
        int defaultWidth = 200,
        int defaultHeight = 200)
    {
        _storageService = storageService;
        _logger = logger;
        _defaultWidth = defaultWidth;
        _defaultHeight = defaultHeight;
    }
    
    /// <inheritdoc/>
    public async Task<Stream> GenerateAsync(Stream imageStream, int width, int height, bool maintainAspectRatio = true)
    {
        try
        {
            imageStream.Position = 0;
            
            using var image = await Image.LoadAsync(imageStream);
            
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = maintainAspectRatio ? ResizeMode.Max : ResizeMode.Stretch
            };
            
            image.Mutate(x => x.Resize(resizeOptions));
            
            var outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new JpegEncoder { Quality = 80 });
            outputStream.Position = 0;
            
            _logger.LogInformation("썸네일 생성 완료: {Width}x{Height}", image.Width, image.Height);
            return outputStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "썸네일 생성 실패");
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<string> GenerateAndSaveAsync(Stream imageStream, string storagePath, int width, int height, bool maintainAspectRatio = true)
    {
        try
        {
            using var thumbnailStream = await GenerateAsync(imageStream, width, height, maintainAspectRatio);
            
            var thumbnailFileName = GetThumbnailFileName(storagePath);
            var savedPath = await _storageService.SaveAsync(thumbnailStream, thumbnailFileName, "image/jpeg", "thumbnails");
            
            _logger.LogInformation("썸네일 저장 완료: {Path}", savedPath);
            return savedPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "썸네일 저장 실패: {StoragePath}", storagePath);
            throw;
        }
    }
    
    /// <inheritdoc/>
    public async Task<(int Width, int Height)> GetImageDimensionsAsync(Stream imageStream)
    {
        try
        {
            imageStream.Position = 0;
            
            var imageInfo = await Image.IdentifyAsync(imageStream);
            
            if (imageInfo == null)
            {
                throw new InvalidOperationException("이미지 정보를 읽을 수 없습니다.");
            }
            
            return (imageInfo.Width, imageInfo.Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "이미지 크기 조회 실패");
            throw;
        }
    }
    
    /// <inheritdoc/>
    public bool CanProcess(string contentType)
    {
        return SupportedImageFormats.Contains(contentType);
    }
    
    /// <inheritdoc/>
    public IReadOnlyList<string> GetSupportedFormats()
    {
        return SupportedImageFormats.ToList().AsReadOnly();
    }
    
    /// <inheritdoc/>
    public (int Width, int Height) GetDefaultThumbnailSize()
    {
        return (_defaultWidth, _defaultHeight);
    }
    
    /// <summary>
    /// 원본 경로로부터 썸네일 파일명 생성
    /// </summary>
    private static string GetThumbnailFileName(string originalPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(originalPath);
        return $"{fileName}_thumb.jpg";
    }
}
