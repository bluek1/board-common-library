using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BoardCommonLibrary.Data;
using BoardCommonLibrary.Extensions;
using BoardDemo.Api.Data;
using BoardDemo.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// InMemory 데이터베이스 설정 - ApplicationDbContext (사용자/인증)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("BoardDemoDb"));

// BoardCommonLibrary 전용 InMemory DbContext 등록
builder.Services.AddDbContext<BoardDbContext>(options =>
    options.UseInMemoryDatabase("BoardDemoDb"));

// BoardCommonLibrary 서비스 등록 (DbContext는 위에서 따로 등록했으므로 ConnectionString 없이)
builder.Services.AddBoardLibrary(options =>
{
    // DB는 위에서 직접 등록, 여기서는 파일 업로드 설정만
    options.FileUpload.StoragePath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
    options.FileUpload.MaxFileSize = 10 * 1024 * 1024; // 10MB
    options.FileUpload.AllowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
    options.FileUpload.ApiBaseUrl = "/api/files";
});

// JWT 인증 설정
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 인증 서비스 등록
builder.Services.AddScoped<IAuthService, AuthService>();

// CORS 설정
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 컨트롤러 및 Swagger 설정
builder.Services.AddControllers()
    .AddApplicationPart(typeof(BoardCommonLibrary.Controllers.PostsController).Assembly); // BoardCommonLibrary 컨트롤러 등록
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Board Demo API",
        Version = "v1",
        Description = "게시판 공통 라이브러리 테스트용 API"
    });
    
    // JWT 인증 설정
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// 데이터베이스 생성 및 시드 데이터
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var boardContext = scope.ServiceProvider.GetRequiredService<BoardDbContext>();
    
    // SQLite DB 생성 - BoardDbContext 먼저 생성 (더 많은 테이블)
    boardContext.Database.EnsureCreated();
    context.Database.EnsureCreated();
    
    // 시드 데이터 생성
    await SeedData.InitializeAsync(context, boardContext);
}

// 개발 환경에서 Swagger 활성화
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Board Demo API v1");
        options.RoutePrefix = string.Empty; // 루트에서 Swagger UI 접근
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// 정적 파일 (업로드된 파일)
var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}
app.UseStaticFiles();

app.MapControllers();

app.Run();
