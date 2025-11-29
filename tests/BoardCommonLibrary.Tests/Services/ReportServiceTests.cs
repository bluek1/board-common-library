using BoardCommonLibrary.Data;
using BoardCommonLibrary.DTOs;
using BoardCommonLibrary.Entities;
using BoardCommonLibrary.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BoardCommonLibrary.Tests.Services;

/// <summary>
/// ReportService 단위 테스트
/// </summary>
public class ReportServiceTests : IDisposable
{
    private readonly BoardDbContext _context;
    private readonly ReportService _service;
    private readonly Mock<ILogger<ReportService>> _loggerMock;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<BoardDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BoardDbContext(options);
        _loggerMock = new Mock<ILogger<ReportService>>();
        _service = new ReportService(_context, _loggerMock.Object);

        // 신고 대상 테스트 데이터 생성
        _context.Posts.Add(new Post
        {
            Id = 1,
            Title = "신고 대상 게시물",
            Content = "내용",
            AuthorId = 1,
            AuthorName = "작성자"
        });
        _context.Comments.Add(new Comment
        {
            Id = 1,
            Content = "신고 대상 댓글",
            PostId = 1,
            AuthorId = 1,
            AuthorName = "작성자"
        });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateReport()
    {
        // Arrange
        var request = new CreateReportRequest
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            Reason = ReportReason.Spam,
            Description = "스팸 게시물입니다."
        };

        // Act
        var result = await _service.CreateAsync(request, reporterId: 2, reporterName: "신고자");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.TargetType.Should().Be(ReportTargetType.Post);
        result.TargetId.Should().Be(1);
        result.Reason.Should().Be(ReportReason.Spam);
        result.Description.Should().Be("스팸 게시물입니다.");
        result.ReporterId.Should().Be(2);
        result.ReporterName.Should().Be("신고자");
        result.Status.Should().Be(ReportStatus.Pending);
    }

    [Fact]
    public async Task CreateAsync_WithCommentTarget_ShouldCreateReport()
    {
        // Arrange
        var request = new CreateReportRequest
        {
            TargetType = ReportTargetType.Comment,
            TargetId = 1,
            Reason = ReportReason.Harassment,
            Description = "욕설 댓글"
        };

        // Act
        var result = await _service.CreateAsync(request, reporterId: 1, reporterName: "신고자");

        // Assert
        result.TargetType.Should().Be(ReportTargetType.Comment);
        result.TargetId.Should().Be(1);
        result.Reason.Should().Be(ReportReason.Harassment);
    }

    [Fact]
    public async Task CreateAsync_DuplicateReport_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingReport = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(existingReport);
        await _context.SaveChangesAsync();

        var request = new CreateReportRequest
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            Reason = ReportReason.Inappropriate
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateAsync(request, reporterId: 2, reporterName: "신고자"));
    }

    [Fact]
    public async Task CreateAsync_WithNonExistingTarget_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var request = new CreateReportRequest
        {
            TargetType = ReportTargetType.Post,
            TargetId = 999,
            Reason = ReportReason.Spam
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.CreateAsync(request, reporterId: 1, reporterName: "신고자"));
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnReport()
    {
        // Arrange
        var report = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByIdAsync(report.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(report.Id);
        result.TargetType.Should().Be(ReportTargetType.Post);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WithReports_ShouldReturnPagedResult()
    {
        // Arrange
        for (int i = 0; i < 15; i++)
        {
            _context.Reports.Add(new Report
            {
                TargetType = ReportTargetType.Post,
                TargetId = 1,
                ReporterId = i + 10,
                ReporterName = $"신고자{i}",
                Reason = ReportReason.Spam,
                Status = ReportStatus.Pending
            });
        }
        await _context.SaveChangesAsync();

        var parameters = new ReportQueryParameters { Page = 1, PageSize = 10 };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(10);
        result.Meta.TotalCount.Should().Be(15);
        result.Meta.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Reports.AddRange(
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 10, ReporterName = "신고자1", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 11, ReporterName = "신고자2", Reason = ReportReason.Spam, Status = ReportStatus.Approved },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 12, ReporterName = "신고자3", Reason = ReportReason.Spam, Status = ReportStatus.Pending }
        );
        await _context.SaveChangesAsync();

        var parameters = new ReportQueryParameters { Status = ReportStatus.Pending };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(r => r.Status == ReportStatus.Pending);
    }

    [Fact]
    public async Task GetAllAsync_FilterByTargetType_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Reports.AddRange(
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 10, ReporterName = "신고자1", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Comment, TargetId = 1, ReporterId = 11, ReporterName = "신고자2", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 12, ReporterName = "신고자3", Reason = ReportReason.Spam, Status = ReportStatus.Pending }
        );
        await _context.SaveChangesAsync();

        var parameters = new ReportQueryParameters { TargetType = ReportTargetType.Post };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(r => r.TargetType == ReportTargetType.Post);
    }

    [Fact]
    public async Task GetAllAsync_FilterByReason_ShouldReturnFilteredResults()
    {
        // Arrange
        _context.Reports.AddRange(
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 10, ReporterName = "신고자1", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 11, ReporterName = "신고자2", Reason = ReportReason.Harassment, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 12, ReporterName = "신고자3", Reason = ReportReason.Spam, Status = ReportStatus.Pending }
        );
        await _context.SaveChangesAsync();

        var parameters = new ReportQueryParameters { Reason = ReportReason.Spam };

        // Act
        var result = await _service.GetAllAsync(parameters);

        // Assert
        result.Data.Should().HaveCount(2);
        result.Data.Should().OnlyContain(r => r.Reason == ReportReason.Spam);
    }

    #endregion

    #region ProcessAsync Tests

    [Fact]
    public async Task ProcessAsync_ApproveReport_ShouldUpdateStatus()
    {
        // Arrange
        var report = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var request = new ProcessReportRequest
        {
            Status = ReportStatus.Approved,
            ProcessingNote = "신고 승인"
        };

        // Act
        var result = await _service.ProcessAsync(report.Id, request, processedById: 1, processedByName: "관리자");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ReportStatus.Approved);
        result.ProcessingNote.Should().Be("신고 승인");
        result.ProcessedById.Should().Be(1);
        result.ProcessedByName.Should().Be("관리자");
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessAsync_RejectReport_ShouldUpdateStatus()
    {
        // Arrange
        var report = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var request = new ProcessReportRequest
        {
            Status = ReportStatus.Rejected,
            ProcessingNote = "허위 신고"
        };

        // Act
        var result = await _service.ProcessAsync(report.Id, request, processedById: 1, processedByName: "관리자");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(ReportStatus.Rejected);
    }

    [Fact]
    public async Task ProcessAsync_WithNonExistingReport_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var request = new ProcessReportRequest
        {
            Status = ReportStatus.Approved
        };

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _service.ProcessAsync(999, request, processedById: 1, processedByName: "관리자"));
    }

    [Fact]
    public async Task ProcessAsync_AlreadyProcessedReport_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var report = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Approved,
            ProcessedById = 1,
            ProcessedByName = "관리자",
            ProcessedAt = DateTime.UtcNow
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        var request = new ProcessReportRequest
        {
            Status = ReportStatus.Rejected
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ProcessAsync(report.Id, request, processedById: 1, processedByName: "관리자"));
    }

    #endregion

    #region GetReportCountAsync Tests

    [Fact]
    public async Task GetReportCountAsync_WithReports_ShouldReturnCount()
    {
        // Arrange
        _context.Reports.AddRange(
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 10, ReporterName = "신고자1", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 11, ReporterName = "신고자2", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 12, ReporterName = "신고자3", Reason = ReportReason.Spam, Status = ReportStatus.Approved }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetReportCountAsync(ReportTargetType.Post, targetId: 1);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task GetReportCountAsync_WithNoReports_ShouldReturnZero()
    {
        // Act
        var result = await _service.GetReportCountAsync(ReportTargetType.Post, targetId: 999);

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region HasReportedAsync Tests

    [Fact]
    public async Task HasReportedAsync_WithExistingReport_ShouldReturnTrue()
    {
        // Arrange
        var report = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HasReportedAsync(ReportTargetType.Post, targetId: 1, reporterId: 2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasReportedAsync_WithNoReport_ShouldReturnFalse()
    {
        // Act
        var result = await _service.HasReportedAsync(ReportTargetType.Post, targetId: 1, reporterId: 2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasReportedAsync_DifferentReporter_ShouldReturnFalse()
    {
        // Arrange
        var report = new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 2,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Pending
        };
        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.HasReportedAsync(ReportTargetType.Post, targetId: 1, reporterId: 3);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region GetPendingCountAsync Tests

    [Fact]
    public async Task GetPendingCountAsync_WithPendingReports_ShouldReturnCount()
    {
        // Arrange
        _context.Reports.AddRange(
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 10, ReporterName = "신고자1", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 11, ReporterName = "신고자2", Reason = ReportReason.Spam, Status = ReportStatus.Pending },
            new Report { TargetType = ReportTargetType.Post, TargetId = 1, ReporterId = 12, ReporterName = "신고자3", Reason = ReportReason.Spam, Status = ReportStatus.Approved }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingCountAsync();

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task GetPendingCountAsync_WithNoPendingReports_ShouldReturnZero()
    {
        // Arrange
        _context.Reports.Add(new Report
        {
            TargetType = ReportTargetType.Post,
            TargetId = 1,
            ReporterId = 10,
            ReporterName = "신고자",
            Reason = ReportReason.Spam,
            Status = ReportStatus.Approved
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingCountAsync();

        // Assert
        result.Should().Be(0);
    }

    #endregion

    #region TargetExistsAsync Tests

    [Fact]
    public async Task TargetExistsAsync_WithExistingPost_ShouldReturnTrue()
    {
        // Act
        var result = await _service.TargetExistsAsync(ReportTargetType.Post, targetId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TargetExistsAsync_WithExistingComment_ShouldReturnTrue()
    {
        // Act
        var result = await _service.TargetExistsAsync(ReportTargetType.Comment, targetId: 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TargetExistsAsync_WithNonExistingPost_ShouldReturnFalse()
    {
        // Act
        var result = await _service.TargetExistsAsync(ReportTargetType.Post, targetId: 999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
