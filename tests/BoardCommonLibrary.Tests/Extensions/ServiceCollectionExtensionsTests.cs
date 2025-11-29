using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Extensions;
using BoardCommonLibrary.Services;
using BoardCommonLibrary.Services.Interfaces;
using BoardCommonLibrary.Validators;
using FluentAssertions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BoardCommonLibrary.Tests.Extensions;

/// <summary>
/// ServiceCollectionExtensions 단위 테스트
/// </summary>
public class ServiceCollectionExtensionsTests
{
    #region AddBoardLibrary Tests

    [Fact]
    public void AddBoardLibrary_WithInMemoryDatabase_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBoardLibrary(options =>
        {
            options.UseInMemoryDatabase = true;
            options.InMemoryDatabaseName = "TestDb";
        });
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<BoardDbContext>().Should().NotBeNull();
        provider.GetService<IPostService>().Should().NotBeNull();
        provider.GetService<IViewCountService>().Should().NotBeNull();
    }

    [Fact]
    public void AddBoardLibrary_ShouldRegisterValidators()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBoardLibrary(options =>
        {
            options.UseInMemoryDatabase = true;
        });
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<IValidator<CreatePostRequest>>().Should().NotBeNull();
        provider.GetService<IValidator<UpdatePostRequest>>().Should().NotBeNull();
        provider.GetService<IValidator<DraftPostRequest>>().Should().NotBeNull();
    }

    [Fact]
    public void AddBoardLibrary_WithConnectionString_ShouldRegisterDbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - ConnectionString 설정하지만 실제 연결은 테스트 안 함
        services.AddBoardLibrary(options =>
        {
            options.ConnectionString = "Server=localhost;Database=TestDb;";
            options.UseInMemoryDatabase = true; // InMemory 사용으로 오버라이드
        });
        var provider = services.BuildServiceProvider();

        // Assert
        provider.GetService<BoardDbContext>().Should().NotBeNull();
    }

    [Fact]
    public void AddBoardLibrary_WithNullOptions_ShouldRegisterValidatorsButNotDbContext()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - 옵션 없이 호출 (DbContext 등록 안 됨)
        services.AddBoardLibrary();
        var provider = services.BuildServiceProvider();

        // Assert - Validator는 등록되지만 DbContext가 없어서 서비스 resolve 불가
        // DbContext 없이 서비스 등록 시 IPostService resolve 시도하면 예외 발생
        provider.GetService<IValidator<CreatePostRequest>>().Should().NotBeNull();
        
        // IPostService는 DbContext 의존성 때문에 resolve 시 예외 발생
        Action act = () => provider.GetService<IPostService>();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AddBoardLibrary_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBoardLibrary(options =>
        {
            options.UseInMemoryDatabase = true;
        });

        // Assert
        result.Should().BeSameAs(services);
    }

    #endregion

    #region AddBoardLibraryInMemory Tests

    [Fact]
    public void AddBoardLibraryInMemory_ShouldRegisterInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBoardLibraryInMemory("CustomTestDb");
        var provider = services.BuildServiceProvider();

        // Assert
        var context = provider.GetService<BoardDbContext>();
        context.Should().NotBeNull();
    }

    [Fact]
    public void AddBoardLibraryInMemory_WithDefaultName_ShouldUseBoardTestDb()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddBoardLibraryInMemory();
        var provider = services.BuildServiceProvider();

        // Assert
        var context = provider.GetService<BoardDbContext>();
        context.Should().NotBeNull();
    }

    [Fact]
    public void AddBoardLibraryInMemory_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddBoardLibraryInMemory();

        // Assert
        result.Should().BeSameAs(services);
    }

    #endregion

    #region BoardLibraryOptions Tests

    [Fact]
    public void BoardLibraryOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new BoardLibraryOptions();

        // Assert
        options.ConnectionString.Should().BeNull();
        options.UseInMemoryDatabase.Should().BeFalse();
        options.InMemoryDatabaseName.Should().BeNull();
        options.ApiPrefix.Should().Be("/api");
        options.ApiVersion.Should().Be("v1");
        options.IncludeVersionInUrl.Should().BeFalse();
    }

    [Fact]
    public void BoardLibraryOptions_ShouldAllowSettingProperties()
    {
        // Arrange
        var options = new BoardLibraryOptions();

        // Act
        options.ConnectionString = "Server=test;Database=test;";
        options.UseInMemoryDatabase = true;
        options.InMemoryDatabaseName = "TestDb";
        options.ApiPrefix = "/custom-api";
        options.ApiVersion = "v2";
        options.IncludeVersionInUrl = true;

        // Assert
        options.ConnectionString.Should().Be("Server=test;Database=test;");
        options.UseInMemoryDatabase.Should().BeTrue();
        options.InMemoryDatabaseName.Should().Be("TestDb");
        options.ApiPrefix.Should().Be("/custom-api");
        options.ApiVersion.Should().Be("v2");
        options.IncludeVersionInUrl.Should().BeTrue();
    }

    #endregion

    #region Service Resolution Tests

    [Fact]
    public void AddBoardLibrary_IPostService_ShouldResolveToPostService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();
        var provider = services.BuildServiceProvider();

        // Act
        var postService = provider.GetService<IPostService>();

        // Assert
        postService.Should().BeOfType<PostService>();
    }

    [Fact]
    public void AddBoardLibrary_IViewCountService_ShouldResolveToViewCountService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();
        var provider = services.BuildServiceProvider();

        // Act
        var viewCountService = provider.GetService<IViewCountService>();

        // Assert
        viewCountService.Should().BeOfType<ViewCountService>();
    }

    [Fact]
    public void AddBoardLibrary_CreatePostRequestValidator_ShouldResolve()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();
        var provider = services.BuildServiceProvider();

        // Act
        var validator = provider.GetService<IValidator<CreatePostRequest>>();

        // Assert
        validator.Should().BeOfType<CreatePostRequestValidator>();
    }

    [Fact]
    public void AddBoardLibrary_UpdatePostRequestValidator_ShouldResolve()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();
        var provider = services.BuildServiceProvider();

        // Act
        var validator = provider.GetService<IValidator<UpdatePostRequest>>();

        // Assert
        validator.Should().BeOfType<UpdatePostRequestValidator>();
    }

    [Fact]
    public void AddBoardLibrary_DraftPostRequestValidator_ShouldResolve()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();
        var provider = services.BuildServiceProvider();

        // Act
        var validator = provider.GetService<IValidator<DraftPostRequest>>();

        // Assert
        validator.Should().BeOfType<DraftPostRequestValidator>();
    }

    #endregion

    #region Scoped Service Tests

    [Fact]
    public void AddBoardLibrary_Services_ShouldBeScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();

        // Act
        var postServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IPostService));
        var viewCountServiceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IViewCountService));

        // Assert
        postServiceDescriptor.Should().NotBeNull();
        postServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        
        viewCountServiceDescriptor.Should().NotBeNull();
        viewCountServiceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddBoardLibrary_Validators_ShouldBeScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddBoardLibraryInMemory();

        // Act
        var validatorDescriptor = services.FirstOrDefault(s => 
            s.ServiceType == typeof(IValidator<CreatePostRequest>));

        // Assert
        validatorDescriptor.Should().NotBeNull();
        validatorDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    #endregion
}
