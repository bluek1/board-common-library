namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 파일 저장소 서비스 인터페이스
/// 로컬 파일 시스템 또는 클라우드 스토리지 추상화
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// 파일 저장
    /// </summary>
    /// <param name="stream">파일 스트림</param>
    /// <param name="fileName">저장할 파일명</param>
    /// <param name="contentType">MIME 타입</param>
    /// <param name="subFolder">하위 폴더 (선택)</param>
    /// <returns>저장된 파일 경로</returns>
    Task<string> SaveAsync(Stream stream, string fileName, string contentType, string? subFolder = null);
    
    /// <summary>
    /// 파일 읽기
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <returns>파일 스트림</returns>
    Task<Stream?> ReadAsync(string filePath);
    
    /// <summary>
    /// 파일 삭제
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> DeleteAsync(string filePath);
    
    /// <summary>
    /// 파일 존재 여부 확인
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <returns>존재 여부</returns>
    Task<bool> ExistsAsync(string filePath);
    
    /// <summary>
    /// 파일 복사
    /// </summary>
    /// <param name="sourcePath">원본 경로</param>
    /// <param name="destinationPath">대상 경로</param>
    /// <returns>성공 여부</returns>
    Task<bool> CopyAsync(string sourcePath, string destinationPath);
    
    /// <summary>
    /// 파일 이동
    /// </summary>
    /// <param name="sourcePath">원본 경로</param>
    /// <param name="destinationPath">대상 경로</param>
    /// <returns>성공 여부</returns>
    Task<bool> MoveAsync(string sourcePath, string destinationPath);
    
    /// <summary>
    /// 파일 URL 생성
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <returns>파일 URL</returns>
    string GetUrl(string filePath);
    
    /// <summary>
    /// 파일 크기 조회
    /// </summary>
    /// <param name="filePath">파일 경로</param>
    /// <returns>파일 크기 (bytes)</returns>
    Task<long> GetFileSizeAsync(string filePath);
    
    /// <summary>
    /// 저장소 사용량 조회
    /// </summary>
    /// <returns>사용된 바이트 수</returns>
    Task<long> GetUsedStorageAsync();
    
    /// <summary>
    /// 폴더 내 파일 목록 조회
    /// </summary>
    /// <param name="folderPath">폴더 경로</param>
    /// <returns>파일 경로 목록</returns>
    Task<IEnumerable<string>> ListFilesAsync(string folderPath);
    
    /// <summary>
    /// 폴더 삭제
    /// </summary>
    /// <param name="folderPath">폴더 경로</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> DeleteFolderAsync(string folderPath);
}
