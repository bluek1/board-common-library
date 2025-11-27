using BoardCommonLibrary.Extensions;
using BoardTestWeb.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 서비스 등록
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "게시판 라이브러리 테스트 API",
        Version = "v1",
        Description = "게시판 공통 라이브러리 테스트용 웹서비스"
    });
});

// 게시판 라이브러리 서비스 등록 (InMemory DB 사용)
builder.Services.AddBoardLibraryInMemory("BoardTestDb");

// 테스트 서비스 등록
builder.Services.AddSingleton<TestExecutionService>();

var app = builder.Build();

// 개발 환경 설정
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "테스트 API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapControllers();

app.Run();
