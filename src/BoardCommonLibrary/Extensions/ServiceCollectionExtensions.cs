using BoardCommonLibrary.Configuration;
using BoardCommonLibrary.Conventions;
using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Interfaces;
using BoardCommonLibrary.Services;
using BoardCommonLibrary.Services.Interfaces;
using BoardCommonLibrary.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        
        // 옵션을 싱글톤으로 등록 (컨벤션에서 사용)
        services.AddSingleton(options);
        services.AddSingleton(options.Routes);
        
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
        
        // 서비스 등록 (TryAdd를 사용하여 사용자가 이미 등록한 커스텀 서비스가 우선됨)
        services.TryAddScoped<IPostService, PostService>();
        services.TryAddScoped<IViewCountService, ViewCountService>();
        services.TryAddScoped<ICommentService, CommentService>();
        services.TryAddScoped<ILikeService, LikeService>();
        services.TryAddScoped<IBookmarkService, BookmarkService>();
        
        // 파일 서비스 등록
        services.TryAddSingleton<IFileStorageService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<LocalFileStorageService>>();
            return new LocalFileStorageService(
                options.FileUpload.StoragePath,
                options.FileUpload.BaseUrl,
                logger);
        });
        
        services.TryAddSingleton<IFileValidationService>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<FileValidationService>>();
            return new FileValidationService(
                logger,
                options.FileUpload.MaxFileSize,
                options.FileUpload.AllowedExtensions,
                options.FileUpload.AllowedContentTypes);
        });
        
        services.TryAddScoped<IThumbnailService>(sp =>
        {
            var storageService = sp.GetRequiredService<IFileStorageService>();
            var logger = sp.GetRequiredService<ILogger<ThumbnailService>>();
            return new ThumbnailService(
                storageService,
                logger,
                options.Thumbnail.Width,
                options.Thumbnail.Height);
        });
        
        services.TryAddScoped<IFileService>(sp =>
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
        services.TryAddScoped<ISearchService, SearchService>();
        
        // Q&A 서비스 등록
        services.TryAddScoped<IQuestionService, QuestionService>();
        services.TryAddScoped<IAnswerService, AnswerService>();
        
        // 신고 서비스 등록
        services.TryAddScoped<IReportService, ReportService>();
        
        // 관리자 서비스 등록
        services.TryAddScoped<IAdminService, AdminService>();
        
        // FluentValidation 등록
        services.TryAddScoped<IValidator<CreatePostRequest>, CreatePostRequestValidator>();
        services.TryAddScoped<IValidator<UpdatePostRequest>, UpdatePostRequestValidator>();
        services.TryAddScoped<IValidator<DraftPostRequest>, DraftPostRequestValidator>();
        services.TryAddScoped<IValidator<CreateCommentRequest>, CreateCommentRequestValidator>();
        services.TryAddScoped<IValidator<UpdateCommentRequest>, UpdateCommentRequestValidator>();
        
        // Q&A Validators 등록
        services.TryAddScoped<IValidator<CreateQuestionRequest>, CreateQuestionRequestValidator>();
        services.TryAddScoped<IValidator<UpdateQuestionRequest>, UpdateQuestionRequestValidator>();
        services.TryAddScoped<IValidator<CreateAnswerRequest>, CreateAnswerRequestValidator>();
        services.TryAddScoped<IValidator<UpdateAnswerRequest>, UpdateAnswerRequestValidator>();
        
        // Report Validators 등록
        services.TryAddScoped<IValidator<CreateReportRequest>, CreateReportRequestValidator>();
        services.TryAddScoped<IValidator<ProcessReportRequest>, ProcessReportRequestValidator>();
        
        return services;
    }
    
    /// <summary>
    /// 커스텀 PostService 등록 (라이브러리 기본 서비스 대신 사용)
    /// </summary>
    /// <typeparam name="TService">커스텀 PostService 타입 (PostService를 상속)</typeparam>
    /// <example>
    /// services.AddCustomPostService&lt;MyCustomPostService&gt;();
    /// services.AddBoardLibrary(options => { ... });
    /// </example>
    public static IServiceCollection AddCustomPostService<TService>(this IServiceCollection services)
        where TService : class, IPostService
    {
        services.AddScoped<IPostService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 CommentService 등록
    /// </summary>
    public static IServiceCollection AddCustomCommentService<TService>(this IServiceCollection services)
        where TService : class, ICommentService
    {
        services.AddScoped<ICommentService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 QuestionService 등록
    /// </summary>
    public static IServiceCollection AddCustomQuestionService<TService>(this IServiceCollection services)
        where TService : class, IQuestionService
    {
        services.AddScoped<IQuestionService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 AnswerService 등록
    /// </summary>
    public static IServiceCollection AddCustomAnswerService<TService>(this IServiceCollection services)
        where TService : class, IAnswerService
    {
        services.AddScoped<IAnswerService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 FileService 등록
    /// </summary>
    public static IServiceCollection AddCustomFileService<TService>(this IServiceCollection services)
        where TService : class, IFileService
    {
        services.AddScoped<IFileService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 SearchService 등록
    /// </summary>
    public static IServiceCollection AddCustomSearchService<TService>(this IServiceCollection services)
        where TService : class, ISearchService
    {
        services.AddScoped<ISearchService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 AdminService 등록
    /// </summary>
    public static IServiceCollection AddCustomAdminService<TService>(this IServiceCollection services)
        where TService : class, IAdminService
    {
        services.AddScoped<IAdminService, TService>();
        return services;
    }
    
    /// <summary>
    /// 커스텀 ReportService 등록
    /// </summary>
    public static IServiceCollection AddCustomReportService<TService>(this IServiceCollection services)
        where TService : class, IReportService
    {
        services.AddScoped<IReportService, TService>();
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
    
    /// <summary>
    /// 커스텀 서비스를 일괄 등록하기 위한 확장 메서드
    /// </summary>
    /// <example>
    /// services.AddBoardLibraryWithCustomServices(config =>
    /// {
    ///     config.UseCustomPostService&lt;MyPostService&gt;();
    ///     config.UseCustomCommentService&lt;MyCommentService&gt;();
    /// });
    /// </example>
    public static IServiceCollection AddBoardLibraryWithCustomServices(
        this IServiceCollection services,
        Action<CustomServiceConfiguration> configureServices,
        Action<BoardLibraryOptions>? configureOptions = null)
    {
        var config = new CustomServiceConfiguration(services);
        configureServices(config);
        return services.AddBoardLibrary(configureOptions);
    }
}

/// <summary>
/// 커스텀 서비스 구성 헬퍼 클래스
/// </summary>
public class CustomServiceConfiguration
{
    private readonly IServiceCollection _services;
    
    public CustomServiceConfiguration(IServiceCollection services)
    {
        _services = services;
    }
    
    /// <summary>
    /// 커스텀 PostService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomPostService<TService>()
        where TService : class, IPostService
    {
        _services.AddScoped<IPostService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 CommentService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomCommentService<TService>()
        where TService : class, ICommentService
    {
        _services.AddScoped<ICommentService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 QuestionService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomQuestionService<TService>()
        where TService : class, IQuestionService
    {
        _services.AddScoped<IQuestionService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 AnswerService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomAnswerService<TService>()
        where TService : class, IAnswerService
    {
        _services.AddScoped<IAnswerService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 FileService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomFileService<TService>()
        where TService : class, IFileService
    {
        _services.AddScoped<IFileService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 SearchService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomSearchService<TService>()
        where TService : class, ISearchService
    {
        _services.AddScoped<ISearchService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 AdminService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomAdminService<TService>()
        where TService : class, IAdminService
    {
        _services.AddScoped<IAdminService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 ReportService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomReportService<TService>()
        where TService : class, IReportService
    {
        _services.AddScoped<IReportService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 LikeService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomLikeService<TService>()
        where TService : class, ILikeService
    {
        _services.AddScoped<ILikeService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 BookmarkService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomBookmarkService<TService>()
        where TService : class, IBookmarkService
    {
        _services.AddScoped<IBookmarkService, TService>();
        return this;
    }
    
    /// <summary>
    /// 커스텀 ViewCountService 사용
    /// </summary>
    public CustomServiceConfiguration UseCustomViewCountService<TService>()
        where TService : class, IViewCountService
    {
        _services.AddScoped<IViewCountService, TService>();
        return this;
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
    /// API 경로 설정
    /// 각 컨트롤러의 API 경로를 커스터마이징할 수 있습니다.
    /// </summary>
    /// <example>
    /// options.Routes.Prefix = "api/v1";
    /// options.Routes.Posts = "articles";
    /// // 결과: /api/v1/articles
    /// </example>
    public ApiRouteOptions Routes { get; set; } = new();
    
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

/// <summary>
/// MVC 옵션 확장 메서드
/// </summary>
public static class MvcOptionsExtensions
{
    /// <summary>
    /// 게시판 라이브러리의 API 경로 컨벤션 적용
    /// AddControllers() 이후에 호출해야 합니다.
    /// </summary>
    /// <param name="mvcOptions">MVC 옵션</param>
    /// <param name="routeOptions">API 경로 옵션</param>
    /// <returns>MVC 옵션</returns>
    /// <example>
    /// builder.Services.AddControllers(options => 
    /// {
    ///     options.UseBoardLibraryRoutes(new ApiRouteOptions
    ///     {
    ///         Prefix = "api/v1",
    ///         Posts = "articles",
    ///         Comments = "replies"
    ///     });
    /// });
    /// </example>
    public static MvcOptions UseBoardLibraryRoutes(this MvcOptions mvcOptions, ApiRouteOptions routeOptions)
    {
        mvcOptions.Conventions.Add(new BoardControllerRouteConvention(routeOptions));
        return mvcOptions;
    }
}
