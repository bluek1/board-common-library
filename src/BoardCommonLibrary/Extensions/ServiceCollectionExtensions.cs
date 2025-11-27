using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Services;
using BoardCommonLibrary.Services.Interfaces;
using BoardCommonLibrary.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
        
        // FluentValidation 등록
        services.AddScoped<IValidator<CreatePostRequest>, CreatePostRequestValidator>();
        services.AddScoped<IValidator<UpdatePostRequest>, UpdatePostRequestValidator>();
        services.AddScoped<IValidator<DraftPostRequest>, DraftPostRequestValidator>();
        
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
}
