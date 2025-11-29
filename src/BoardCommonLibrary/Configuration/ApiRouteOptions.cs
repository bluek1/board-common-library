namespace BoardCommonLibrary.Configuration;

/// <summary>
/// API 경로 옵션
/// 각 컨트롤러의 API 경로를 사용자가 커스터마이징할 수 있습니다.
/// </summary>
public class ApiRouteOptions
{
    /// <summary>
    /// API 기본 접두사 (기본값: "api")
    /// </summary>
    /// <example>
    /// "api" -> /api/posts
    /// "api/v1" -> /api/v1/posts
    /// "board-api" -> /board-api/posts
    /// </example>
    public string Prefix { get; set; } = "api";
    
    /// <summary>
    /// 게시물 API 경로 (기본값: "posts")
    /// </summary>
    /// <example>
    /// "posts" -> /api/posts
    /// "articles" -> /api/articles
    /// "board/posts" -> /api/board/posts
    /// </example>
    public string Posts { get; set; } = "posts";
    
    /// <summary>
    /// 댓글 API 경로 (기본값: "comments")
    /// </summary>
    public string Comments { get; set; } = "comments";
    
    /// <summary>
    /// 파일 API 경로 (기본값: "files")
    /// </summary>
    public string Files { get; set; } = "files";
    
    /// <summary>
    /// 검색 API 경로 (기본값: "search")
    /// </summary>
    public string Search { get; set; } = "search";
    
    /// <summary>
    /// 사용자 API 경로 (기본값: "users")
    /// </summary>
    public string Users { get; set; } = "users";
    
    /// <summary>
    /// Q&A 질문 API 경로 (기본값: "questions")
    /// </summary>
    public string Questions { get; set; } = "questions";
    
    /// <summary>
    /// Q&A 답변 API 경로 (기본값: "answers")
    /// </summary>
    public string Answers { get; set; } = "answers";
    
    /// <summary>
    /// 신고 API 경로 (기본값: "reports")
    /// </summary>
    public string Reports { get; set; } = "reports";
    
    /// <summary>
    /// 관리자 API 경로 (기본값: "admin")
    /// </summary>
    public string Admin { get; set; } = "admin";
    
    /// <summary>
    /// 컨트롤러 이름으로 전체 경로 가져오기
    /// </summary>
    /// <param name="controllerName">컨트롤러 이름 (예: "Posts", "Comments")</param>
    /// <returns>전체 API 경로 (예: "api/posts")</returns>
    public string GetRoute(string controllerName)
    {
        var route = controllerName.ToLowerInvariant() switch
        {
            "posts" => Posts,
            "comments" => Comments,
            "files" => Files,
            "search" => Search,
            "users" => Users,
            "questions" => Questions,
            "answers" => Answers,
            "reports" => Reports,
            "admin" => Admin,
            _ => controllerName.ToLowerInvariant()
        };
        
        return string.IsNullOrEmpty(Prefix) ? route : $"{Prefix}/{route}";
    }
}
