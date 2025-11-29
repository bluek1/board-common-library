using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using BoardCommonLibrary.Services;
using BoardCommonLibrary.Services.Interfaces;
using BoardCommonLibrary.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BoardCommonLibrary.Extensions;

/// <summary>
/// 서비스 컬렉션 확장 메서드
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 게시판 라이브러리 서비스 등록
    /// </summary>
    /// <param name="services">서비스 컬렉션</param>
    /// <param name="configureOptions">옵션 설정 액션</param>
    /// <returns>서비스 컬렉션</returns>
    public static IServiceCollection AddBoardLibrary(
        this IServiceCollection services, 
        Action<BoardLibraryOptions>? configureOptions = null)
    {
        var options = new BoardLibraryOptions();
        configureOptions?.Invoke(options);
        
        // DbContext 등록
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            services.AddDbContext<BoardDbContext>(opt =>
            {
                if (options.UseInMemoryDatabase)
                {
                    opt.UseInMemoryDatabase(options.InMemoryDatabaseName ?? "BoardTestDb");
                }
                else
                {
                    opt.UseSqlServer(options.ConnectionString);
                }
            });
        }
        else if (options.UseInMemoryDatabase)
        {
            services.AddDbContext<BoardDbContext>(opt =>
                opt.UseInMemoryDatabase(options.InMemoryDatabaseName ?? "BoardTestDb"));
        }
        
        // 서비스 등록
        services.AddScoped<IPostService, PostService>();
        services.AddScoped<IViewCountService, ViewCountService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<ILikeService, LikeService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        
        // 파일 서비스 등록
        services.AddSingleton<IFileStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<LocalFileStorageService>>();
            return new LocalFileStorageService(
                options.FileUpload.StoragePath,
                options.FileUpload.BaseUrl,
                logger);
        });
        
        services.AddSingleton<IFileValidationService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FileValidationService>>();
            return new FileValidationService(
                logger,
                options.FileUpload.MaxFileSize,
                options.FileUpload.AllowedExtensions,
                options.FileUpload.AllowedContentTypes);
        });
        
        services.AddScoped<IThumbnailService>(sp =>
        {
            var storageService = sp.GetRequiredService<IFileStorageService>();
            var logger = sp.GetRequiredService<ILogger<ThumbnailService>>();
            return new ThumbnailService(
                storageService,
                logger,
                options.Thumbnail.Width,
                options.Thumbnail.Height);
        });
        
        services.AddScoped<IFileService>(sp =>
        {
            var context = sp.GetRequiredService<BoardDbContext>();
            var storageService = sp.GetRequiredService<IFileStorageService>();
            var validationService = sp.GetRequiredService<IFileValidationService>();
            var thumbnailService = sp.GetRequiredService<IThumbnailService>();
            var logger = sp.GetRequiredService<ILogger<FileService>>();
            return new FileService(
                context,
                storageService,
                validationService,
                thumbnailService,
                logger,
                options.FileUpload.ApiBaseUrl);
        });
        
        // 검색 서비스 등록
        services.AddScoped<ISearchService, SearchService>();
        
        // Q&A 서비스 등록
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IAnswerService, AnswerService>();
        
        // 신고 서비스 등록
        services.AddScoped<IReportService, ReportService>();
        
        // 관리자 서비스 등록
        services.AddScoped<IAdminService, AdminService>();
        
        // FluentValidation 등록
        services.AddScoped<IValidator<CreatePostRequest>, CreatePostRequestValidator>();
        services.AddScoped<IValidator<UpdatePostRequest>, UpdatePostRequestValidator>();
        services.AddScoped<IValidator<DraftPostRequest>, DraftPostRequestValidator>();
        services.AddScoped<IValidator<CreateCommentRequest>, CreateCommentRequestValidator>();
        services.AddScoped<IValidator<UpdateCommentRequest>, UpdateCommentRequestValidator>();
        
        // Q&A Validators 등록
        services.AddScoped<IValidator<CreateQuestionRequest>, CreateQuestionRequestValidator>();
        services.AddScoped<IValidator<UpdateQuestionRequest>, UpdateQuestionRequestValidator>();
        services.AddScoped<IValidator<CreateAnswerRequest>, CreateAnswerRequestValidator>();
        services.AddScoped<IValidator<UpdateAnswerRequest>, UpdateAnswerRequestValidator>();
        
        // Report Validators 등록
        services.AddScoped<IValidator<CreateReportRequest>, CreateReportRequestValidator>();
        services.AddScoped<IValidator<ProcessReportRequest>, ProcessReportRequestValidator>();
        
        return services;
    }
    
    /// <summary>
    /// 게시판 라이브러리 서비스 등록 (InMemory DB 사용)
    /// </summary>
    public static IServiceCollection AddBoardLibraryInMemory(
        this IServiceCollection services, 
        string databaseName = "BoardTestDb")
    {
        return services.AddBoardLibrary(options =>
        {
            options.UseInMemoryDatabase = true;
            options.InMemoryDatabaseName = databaseName;
        });
    }
}

/// <summary>
/// 게시판 라이브러리 옵션
/// </summary>
public class BoardLibraryOptions
{
    /// <summary>
    /// 데이터베이스 연결 문자열
    /// </summary>
    public string? ConnectionString { get; set; }
    
    /// <summary>
    /// InMemory 데이터베이스 사용 여부 (테스트용)
    /// </summary>
    public bool UseInMemoryDatabase { get; set; }
    
    /// <summary>
    /// InMemory 데이터베이스 이름
    /// </summary>
    public string? InMemoryDatabaseName { get; set; }
    
    /// <summary>
    /// API 접두사 (기본값: /api)
    /// </summary>
    public string ApiPrefix { get; set; } = "/api";
    
    /// <summary>
    /// API 버전 (기본값: v1)
    /// </summary>
    public string ApiVersion { get; set; } = "v1";
    
    /// <summary>
    /// URL에 버전 포함 여부
    /// </summary>
    public bool IncludeVersionInUrl { get; set; } = false;
    
    /// <summary>
    /// 파일 업로드 설정
    /// </summary>
    public FileUploadOptions FileUpload { get; set; } = new();
    
    /// <summary>
    /// 썸네일 설정
    /// </summary>
    public ThumbnailOptions Thumbnail { get; set; } = new();
}

/// <summary>
/// 파일 업로드 옵션
/// </summary>
public class FileUploadOptions
{
    /// <summary>
    /// 파일 저장 경로
    /// </summary>
    public string StoragePath { get; set; } = "./uploads";
    
    /// <summary>
    /// 파일 URL 기본 경로
    /// </summary>
    public string BaseUrl { get; set; } = "/files";
    
    /// <summary>
    /// 파일 API 기본 URL
    /// </summary>
    public string ApiBaseUrl { get; set; } = "/api/files";
    
    /// <summary>
    /// 최대 파일 크기 (bytes, 기본 10MB)
    /// </summary>
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    
    /// <summary>
    /// 허용된 확장자 목록
    /// </summary>
    public List<string>? AllowedExtensions { get; set; }
    
    /// <summary>
    /// 허용된 MIME 타입 목록
    /// </summary>
    public List<string>? AllowedContentTypes { get; set; }
}

/// <summary>
/// 썸네일 옵션
/// </summary>
public class ThumbnailOptions
{
    /// <summary>
    /// 썸네일 너비 (기본 200px)
    /// </summary>
    public int Width { get; set; } = 200;
    
    /// <summary>
    /// 썸네일 높이 (기본 200px)
    /// </summary>
    public int Height { get; set; } = 200;
}
