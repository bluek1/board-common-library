namespace BoardCommonLibrary.DTOs;

/// <summary>
/// API 성공 응답
/// </summary>
/// <typeparam name="T">데이터 타입</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 성공 여부
    /// </summary>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// 응답 데이터
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// 성공 응답 생성
    /// </summary>
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T> { Data = data };
    }
}

/// <summary>
/// API 에러 응답
/// </summary>
public class ApiErrorResponse
{
    /// <summary>
    /// 성공 여부
    /// </summary>
    public bool Success { get; set; } = false;
    
    /// <summary>
    /// 에러 정보
    /// </summary>
    public ApiError Error { get; set; } = new();
    
    /// <summary>
    /// 에러 응답 생성
    /// </summary>
    public static ApiErrorResponse Create(string code, string message, List<ValidationError>? details = null)
    {
        return new ApiErrorResponse
        {
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}

/// <summary>
/// API 에러 상세
/// </summary>
public class ApiError
{
    /// <summary>
    /// 에러 코드
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 에러 메시지
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 유효성 검증 에러 상세 (선택)
    /// </summary>
    public List<ValidationError>? Details { get; set; }
}

/// <summary>
/// 유효성 검증 에러
/// </summary>
public class ValidationError
{
    /// <summary>
    /// 필드명
    /// </summary>
    public string Field { get; set; } = string.Empty;
    
    /// <summary>
    /// 에러 메시지
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
