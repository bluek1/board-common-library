using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using Microsoft.AspNetCore.Http;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 파일 관리 서비스 인터페이스
/// </summary>
public interface IFileService
{
    /// <summary>
    /// 단일 파일 업로드
    /// </summary>
    /// <param name="file">업로드할 파일</param>
    /// <param name="uploaderId">업로더 ID</param>
    /// <param name="uploaderName">업로더 이름</param>
    /// <param name="postId">연결할 게시물 ID (선택)</param>
    /// <returns>업로드된 파일 정보</returns>
    Task<FileInfoResponse> UploadAsync(IFormFile file, long uploaderId, string uploaderName, long? postId = null);
    
    /// <summary>
    /// 다중 파일 업로드
    /// </summary>
    /// <param name="files">업로드할 파일 목록</param>
    /// <param name="uploaderId">업로더 ID</param>
    /// <param name="uploaderName">업로더 이름</param>
    /// <param name="postId">연결할 게시물 ID (선택)</param>
    /// <returns>업로드 결과</returns>
    Task<MultipleFileUploadResponse> UploadMultipleAsync(List<IFormFile> files, long uploaderId, string uploaderName, long? postId = null);
    
    /// <summary>
    /// 파일 정보 조회
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>파일 정보</returns>
    Task<FileInfoResponse?> GetByIdAsync(long id);
    
    /// <summary>
    /// 파일 다운로드
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>파일 다운로드 응답</returns>
    Task<FileDownloadResponse?> DownloadAsync(long id);
    
    /// <summary>
    /// 썸네일 다운로드
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>썸네일 다운로드 응답</returns>
    Task<FileDownloadResponse?> GetThumbnailAsync(long id);
    
    /// <summary>
    /// 파일 삭제
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <param name="userId">요청 사용자 ID</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> DeleteAsync(long id, long userId);
    
    /// <summary>
    /// 파일 영구 삭제 (관리자용)
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>삭제 성공 여부</returns>
    Task<bool> HardDeleteAsync(long id);
    
    /// <summary>
    /// 게시물의 첨부파일 목록 조회
    /// </summary>
    /// <param name="postId">게시물 ID</param>
    /// <returns>첨부파일 목록</returns>
    Task<List<FileInfoResponse>> GetByPostIdAsync(long postId);
    
    /// <summary>
    /// 사용자의 업로드 파일 목록 조회
    /// </summary>
    /// <param name="uploaderId">업로더 ID</param>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>파일 목록 (페이징)</returns>
    Task<PagedResponse<FileInfoResponse>> GetByUploaderAsync(long uploaderId, FileQueryParameters parameters);
    
    /// <summary>
    /// 파일 목록 조회
    /// </summary>
    /// <param name="parameters">조회 파라미터</param>
    /// <returns>파일 목록 (페이징)</returns>
    Task<PagedResponse<FileInfoResponse>> GetAllAsync(FileQueryParameters parameters);
    
    /// <summary>
    /// 파일을 게시물에 연결
    /// </summary>
    /// <param name="fileId">파일 ID</param>
    /// <param name="postId">게시물 ID</param>
    /// <returns>성공 여부</returns>
    Task<bool> AttachToPostAsync(long fileId, long postId);
    
    /// <summary>
    /// 파일을 게시물에서 분리
    /// </summary>
    /// <param name="fileId">파일 ID</param>
    /// <returns>성공 여부</returns>
    Task<bool> DetachFromPostAsync(long fileId);
    
    /// <summary>
    /// 파일 존재 여부 확인
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <returns>존재 여부</returns>
    Task<bool> ExistsAsync(long id);
    
    /// <summary>
    /// 파일 소유자 확인
    /// </summary>
    /// <param name="id">파일 ID</param>
    /// <param name="userId">사용자 ID</param>
    /// <returns>소유자 여부</returns>
    Task<bool> IsOwnerAsync(long id, long userId);
}
