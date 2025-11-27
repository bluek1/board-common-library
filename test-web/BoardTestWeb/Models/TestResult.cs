namespace BoardTestWeb.Models;

/// <summary>
/// 테스트 결과 모델
/// </summary>
public class TestResult
{
    /// <summary>
    /// 테스트 ID
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// 테스트명
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// 페이지 번호 (1-4)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 테스트 통과 여부
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// 실행 시간 (밀리초)
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 실행 일시
    /// </summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>
    /// 오류 메시지 (실패 시)
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 상세 내용
    /// </summary>
    public string? Details { get; set; }
}

/// <summary>
/// 페이지 테스트 요약
/// </summary>
public class PageTestSummary
{
    /// <summary>
    /// 페이지 번호
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 페이지 제목
    /// </summary>
    public string PageTitle { get; set; } = string.Empty;

    /// <summary>
    /// 전체 테스트 수
    /// </summary>
    public int TotalTests { get; set; }

    /// <summary>
    /// 통과한 테스트 수
    /// </summary>
    public int PassedTests { get; set; }

    /// <summary>
    /// 실패한 테스트 수
    /// </summary>
    public int FailedTests { get; set; }

    /// <summary>
    /// 완료율 (%)
    /// </summary>
    public double CompletionRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;

    /// <summary>
    /// 마지막 실행 일시
    /// </summary>
    public DateTime? LastExecutedAt { get; set; }

    /// <summary>
    /// 개별 테스트 결과 목록
    /// </summary>
    public List<TestResult> TestResults { get; set; } = new();
}

/// <summary>
/// 전체 테스트 상태
/// </summary>
public class TestStatus
{
    /// <summary>
    /// 전체 테스트 수
    /// </summary>
    public int TotalTests { get; set; }

    /// <summary>
    /// 통과한 테스트 수
    /// </summary>
    public int PassedTests { get; set; }

    /// <summary>
    /// 실패한 테스트 수
    /// </summary>
    public int FailedTests { get; set; }

    /// <summary>
    /// 전체 완료율 (%)
    /// </summary>
    public double OverallCompletionRate => TotalTests > 0 ? (double)PassedTests / TotalTests * 100 : 0;

    /// <summary>
    /// 페이지별 요약
    /// </summary>
    public List<PageTestSummary> PageSummaries { get; set; } = new();

    /// <summary>
    /// 마지막 업데이트 일시
    /// </summary>
    public DateTime LastUpdatedAt { get; set; }
}

/// <summary>
/// 테스트 케이스 정의
/// </summary>
public class TestCase
{
    /// <summary>
    /// 테스트 ID
    /// </summary>
    public string TestId { get; set; } = string.Empty;

    /// <summary>
    /// 테스트명
    /// </summary>
    public string TestName { get; set; } = string.Empty;

    /// <summary>
    /// 테스트 설명
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 페이지 번호
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// 예상 결과
    /// </summary>
    public string ExpectedResult { get; set; } = string.Empty;

    /// <summary>
    /// 테스트 상태
    /// </summary>
    public TestCaseStatus Status { get; set; } = TestCaseStatus.대기;
}

/// <summary>
/// 테스트 케이스 상태
/// </summary>
public enum TestCaseStatus
{
    대기,
    진행중,
    완료,
    실패
}
