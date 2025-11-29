namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 썸네일 생성 서비스 인터페이스
/// </summary>
public interface IThumbnailService
{
    /// <summary>
    /// 썸네일 생성
    /// </summary>
    /// <param name="imageStream">원본 이미지 스트림</param>
    /// <param name="width">너비 (픽셀)</param>
    /// <param name="height">높이 (픽셀)</param>
    /// <param name="maintainAspectRatio">종횡비 유지 여부</param>
    /// <returns>썸네일 이미지 스트림</returns>
    Task<Stream> GenerateAsync(Stream imageStream, int width, int height, bool maintainAspectRatio = true);
    
    /// <summary>
    /// 썸네일 생성 및 저장
    /// </summary>
    /// <param name="imageStream">원본 이미지 스트림</param>
    /// <param name="storagePath">저장 경로</param>
    /// <param name="width">너비 (픽셀)</param>
    /// <param name="height">높이 (픽셀)</param>
    /// <param name="maintainAspectRatio">종횡비 유지 여부</param>
    /// <returns>저장된 썸네일 경로</returns>
    Task<string> GenerateAndSaveAsync(Stream imageStream, string storagePath, int width, int height, bool maintainAspectRatio = true);
    
    /// <summary>
    /// 이미지 크기 조회
    /// </summary>
    /// <param name="imageStream">이미지 스트림</param>
    /// <returns>너비, 높이</returns>
    Task<(int Width, int Height)> GetImageDimensionsAsync(Stream imageStream);
    
    /// <summary>
    /// 이미지 처리 가능 여부 확인
    /// </summary>
    /// <param name="contentType">MIME 타입</param>
    /// <returns>처리 가능 여부</returns>
    bool CanProcess(string contentType);
    
    /// <summary>
    /// 지원하는 이미지 형식 목록
    /// </summary>
    /// <returns>지원 형식 목록</returns>
    IReadOnlyList<string> GetSupportedFormats();
    
    /// <summary>
    /// 기본 썸네일 크기 조회
    /// </summary>
    /// <returns>너비, 높이</returns>
    (int Width, int Height) GetDefaultThumbnailSize();
}
