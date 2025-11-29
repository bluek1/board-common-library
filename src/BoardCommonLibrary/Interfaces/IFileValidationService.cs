using BoardCommonLibrary.DTOs;
using Microsoft.AspNetCore.Http;

namespace BoardCommonLibrary.Interfaces;

/// <summary>
/// 파일 검증 서비스 인터페이스
/// </summary>
public interface IFileValidationService
{
    /// <summary>
    /// 파일 검증
    /// </summary>
    /// <param name="file">검증할 파일</param>
    /// <returns>검증 결과</returns>
    Task<FileValidationResult> ValidateAsync(IFormFile file);
    
    /// <summary>
    /// 파일 크기 검증
    /// </summary>
    /// <param name="file">검증할 파일</param>
    /// <param name="maxSize">최대 크기 (bytes)</param>
    /// <returns>유효 여부</returns>
    bool ValidateFileSize(IFormFile file, long? maxSize = null);
    
    /// <summary>
    /// 파일 확장자 검증
    /// </summary>
    /// <param name="fileName">파일명</param>
    /// <returns>유효 여부</returns>
    bool ValidateExtension(string fileName);
    
    /// <summary>
    /// 파일 시그니처 검증 (매직 넘버)
    /// </summary>
    /// <param name="file">검증할 파일</param>
    /// <returns>검증 결과</returns>
    Task<FileValidationResult> ValidateFileSignatureAsync(IFormFile file);
    
    /// <summary>
    /// MIME 타입 검증
    /// </summary>
    /// <param name="contentType">MIME 타입</param>
    /// <returns>유효 여부</returns>
    bool ValidateContentType(string contentType);
    
    /// <summary>
    /// 파일명 검증 (위험한 문자 포함 여부)
    /// </summary>
    /// <param name="fileName">파일명</param>
    /// <returns>유효 여부</returns>
    bool ValidateFileName(string fileName);
    
    /// <summary>
    /// 이미지 파일인지 확인
    /// </summary>
    /// <param name="contentType">MIME 타입</param>
    /// <returns>이미지 여부</returns>
    bool IsImageFile(string contentType);
    
    /// <summary>
    /// 허용된 확장자 목록 조회
    /// </summary>
    /// <returns>허용된 확장자 목록</returns>
    IReadOnlyList<string> GetAllowedExtensions();
    
    /// <summary>
    /// 허용된 MIME 타입 목록 조회
    /// </summary>
    /// <returns>허용된 MIME 타입 목록</returns>
    IReadOnlyList<string> GetAllowedContentTypes();
    
    /// <summary>
    /// 최대 파일 크기 조회
    /// </summary>
    /// <returns>최대 파일 크기 (bytes)</returns>
    long GetMaxFileSize();
    
    /// <summary>
    /// 안전한 파일명 생성
    /// </summary>
    /// <param name="originalFileName">원본 파일명</param>
    /// <returns>안전한 파일명</returns>
    string GenerateSafeFileName(string originalFileName);
    
    /// <summary>
    /// 고유한 저장 파일명 생성
    /// </summary>
    /// <param name="originalFileName">원본 파일명</param>
    /// <returns>고유한 파일명</returns>
    string GenerateUniqueFileName(string originalFileName);
}
