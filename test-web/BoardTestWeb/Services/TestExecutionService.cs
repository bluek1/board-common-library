using BoardTestWeb.Models;
using System.Diagnostics;

namespace BoardTestWeb.Services;

/// <summary>
/// 테스트 실행 서비스
/// </summary>
public class TestExecutionService
{
    private readonly List<TestCase> _testCases;
    private readonly List<TestResult> _testResults;
    private readonly object _lock = new();

    /// <summary>
    /// 페이지 제목 상수
    /// </summary>
    private static readonly Dictionary<int, string> PageTitles = new()
    {
        { 1, "게시물 관리" },
        { 2, "댓글/좋아요" },
        { 3, "파일/검색" },
        { 4, "관리자/Q&A" }
    };

    public TestExecutionService()
    {
        _testCases = InitializeTestCases();
        _testResults = new List<TestResult>();
    }

    /// <summary>
    /// 테스트 케이스 초기화
    /// </summary>
    private List<TestCase> InitializeTestCases()
    {
        return new List<TestCase>
        {
            // 페이지 1: 게시물 관리 테스트
            new TestCase { TestId = "T1-001", TestName = "게시물 작성 성공", Description = "유효한 데이터로 게시물 작성", PageNumber = 1, ExpectedResult = "201 Created, 게시물 ID 반환" },
            new TestCase { TestId = "T1-002", TestName = "게시물 작성 실패 - 제목 없음", Description = "제목 없이 게시물 작성 시도", PageNumber = 1, ExpectedResult = "400 Bad Request" },
            new TestCase { TestId = "T1-003", TestName = "게시물 조회 성공", Description = "존재하는 게시물 ID로 조회", PageNumber = 1, ExpectedResult = "200 OK, 게시물 상세 정보" },
            new TestCase { TestId = "T1-004", TestName = "게시물 조회 실패 - 미존재", Description = "존재하지 않는 ID로 조회", PageNumber = 1, ExpectedResult = "404 Not Found" },
            new TestCase { TestId = "T1-005", TestName = "게시물 수정 성공", Description = "권한 있는 사용자가 게시물 수정", PageNumber = 1, ExpectedResult = "200 OK, 수정된 정보" },
            new TestCase { TestId = "T1-006", TestName = "게시물 수정 실패 - 권한 없음", Description = "작성자가 아닌 사용자가 수정 시도", PageNumber = 1, ExpectedResult = "403 Forbidden" },
            new TestCase { TestId = "T1-007", TestName = "게시물 삭제 성공", Description = "권한 있는 사용자가 게시물 삭제", PageNumber = 1, ExpectedResult = "204 No Content" },
            new TestCase { TestId = "T1-008", TestName = "목록 조회 - 페이징", Description = "페이지네이션 파라미터로 조회", PageNumber = 1, ExpectedResult = "200 OK, 지정된 페이지 데이터" },
            new TestCase { TestId = "T1-009", TestName = "목록 조회 - 정렬", Description = "정렬 조건으로 조회", PageNumber = 1, ExpectedResult = "200 OK, 정렬된 데이터" },
            new TestCase { TestId = "T1-010", TestName = "조회수 증가 확인", Description = "게시물 조회 시 조회수 1 증가", PageNumber = 1, ExpectedResult = "조회수 값 증가 확인" },
            new TestCase { TestId = "T1-011", TestName = "조회수 중복 방지", Description = "같은 사용자가 재조회", PageNumber = 1, ExpectedResult = "조회수 미증가" },
            new TestCase { TestId = "T1-012", TestName = "상단고정 설정", Description = "관리자가 상단고정 설정", PageNumber = 1, ExpectedResult = "200 OK, isPinned = true" },
            new TestCase { TestId = "T1-013", TestName = "상단고정 해제", Description = "관리자가 상단고정 해제", PageNumber = 1, ExpectedResult = "200 OK, isPinned = false" },
            new TestCase { TestId = "T1-014", TestName = "임시저장 성공", Description = "게시물 임시저장", PageNumber = 1, ExpectedResult = "200 OK, draft ID 반환" },
            new TestCase { TestId = "T1-015", TestName = "임시저장 목록", Description = "임시저장 목록 조회", PageNumber = 1, ExpectedResult = "200 OK, draft 목록" },

            // 페이지 2: 댓글/좋아요 테스트
            new TestCase { TestId = "T2-001", TestName = "댓글 작성 성공", Description = "유효한 데이터로 댓글 작성", PageNumber = 2, ExpectedResult = "201 Created" },
            new TestCase { TestId = "T2-002", TestName = "댓글 작성 실패 - 미인증", Description = "비로그인 상태로 댓글 작성", PageNumber = 2, ExpectedResult = "401 Unauthorized" },
            new TestCase { TestId = "T2-003", TestName = "댓글 목록 조회", Description = "게시물의 댓글 목록 조회", PageNumber = 2, ExpectedResult = "200 OK, 댓글 목록" },
            new TestCase { TestId = "T2-004", TestName = "댓글 수정 성공", Description = "작성자가 댓글 수정", PageNumber = 2, ExpectedResult = "200 OK" },
            new TestCase { TestId = "T2-005", TestName = "댓글 수정 실패 - 권한 없음", Description = "타인의 댓글 수정 시도", PageNumber = 2, ExpectedResult = "403 Forbidden" },
            new TestCase { TestId = "T2-006", TestName = "댓글 삭제 성공", Description = "작성자가 댓글 삭제", PageNumber = 2, ExpectedResult = "204 No Content" },
            new TestCase { TestId = "T2-007", TestName = "대댓글 작성 성공", Description = "댓글에 대댓글 작성", PageNumber = 2, ExpectedResult = "201 Created" },
            new TestCase { TestId = "T2-008", TestName = "대댓글 계층 조회", Description = "댓글과 대댓글 계층 구조 조회", PageNumber = 2, ExpectedResult = "계층 구조 확인" },
            new TestCase { TestId = "T2-009", TestName = "좋아요 토글 - 추가", Description = "좋아요 추가", PageNumber = 2, ExpectedResult = "likeCount 증가" },
            new TestCase { TestId = "T2-010", TestName = "좋아요 토글 - 취소", Description = "좋아요 취소", PageNumber = 2, ExpectedResult = "likeCount 감소" },
            new TestCase { TestId = "T2-011", TestName = "좋아요 중복 방지", Description = "같은 게시물에 중복 좋아요", PageNumber = 2, ExpectedResult = "409 Conflict" },
            new TestCase { TestId = "T2-012", TestName = "북마크 추가", Description = "게시물 북마크", PageNumber = 2, ExpectedResult = "200 OK" },
            new TestCase { TestId = "T2-013", TestName = "북마크 해제", Description = "북마크 해제", PageNumber = 2, ExpectedResult = "200 OK" },
            new TestCase { TestId = "T2-014", TestName = "북마크 목록 조회", Description = "내 북마크 목록", PageNumber = 2, ExpectedResult = "200 OK, 북마크 목록" },
            new TestCase { TestId = "T2-015", TestName = "댓글 수 동기화", Description = "댓글 작성/삭제 시 게시물 댓글 수 갱신", PageNumber = 2, ExpectedResult = "commentCount 정확히 반영" },

            // 페이지 3: 파일/검색 테스트
            new TestCase { TestId = "T3-001", TestName = "파일 업로드 성공", Description = "허용된 파일 업로드", PageNumber = 3, ExpectedResult = "201 Created, 파일 ID" },
            new TestCase { TestId = "T3-002", TestName = "파일 업로드 실패 - 크기 초과", Description = "최대 크기 초과 파일", PageNumber = 3, ExpectedResult = "413 Payload Too Large" },
            new TestCase { TestId = "T3-003", TestName = "파일 업로드 실패 - 확장자 불허", Description = "허용되지 않은 확장자", PageNumber = 3, ExpectedResult = "415 Unsupported Media Type" },
            new TestCase { TestId = "T3-004", TestName = "다중 파일 업로드", Description = "여러 파일 동시 업로드", PageNumber = 3, ExpectedResult = "201 Created, 파일 ID 배열" },
            new TestCase { TestId = "T3-005", TestName = "파일 다운로드 성공", Description = "존재하는 파일 다운로드", PageNumber = 3, ExpectedResult = "200 OK, 파일 데이터" },
            new TestCase { TestId = "T3-006", TestName = "파일 다운로드 실패 - 미존재", Description = "존재하지 않는 파일", PageNumber = 3, ExpectedResult = "404 Not Found" },
            new TestCase { TestId = "T3-007", TestName = "파일 삭제 성공", Description = "권한 있는 사용자가 삭제", PageNumber = 3, ExpectedResult = "204 No Content" },
            new TestCase { TestId = "T3-008", TestName = "썸네일 조회", Description = "이미지 파일의 썸네일", PageNumber = 3, ExpectedResult = "200 OK, 썸네일 이미지" },
            new TestCase { TestId = "T3-009", TestName = "제목 검색", Description = "제목에서 키워드 검색", PageNumber = 3, ExpectedResult = "200 OK, 검색 결과" },
            new TestCase { TestId = "T3-010", TestName = "본문 검색", Description = "본문에서 키워드 검색", PageNumber = 3, ExpectedResult = "200 OK, 검색 결과" },
            new TestCase { TestId = "T3-011", TestName = "태그 검색", Description = "태그로 검색", PageNumber = 3, ExpectedResult = "200 OK, 검색 결과" },
            new TestCase { TestId = "T3-012", TestName = "복합 조건 검색", Description = "제목 + 카테고리 조합", PageNumber = 3, ExpectedResult = "200 OK, 필터링된 결과" },
            new TestCase { TestId = "T3-013", TestName = "검색 결과 페이징", Description = "검색 결과 페이지네이션", PageNumber = 3, ExpectedResult = "200 OK, 페이징 적용" },
            new TestCase { TestId = "T3-014", TestName = "검색 결과 하이라이팅", Description = "검색어 강조 표시", PageNumber = 3, ExpectedResult = "검색어 하이라이트 확인" },
            new TestCase { TestId = "T3-015", TestName = "빈 검색 결과", Description = "결과 없는 검색", PageNumber = 3, ExpectedResult = "200 OK, 빈 배열" },

            // 페이지 4: 관리자/Q&A 테스트
            new TestCase { TestId = "T4-001", TestName = "관리자 게시물 조회", Description = "관리자가 전체 게시물 조회", PageNumber = 4, ExpectedResult = "200 OK, 전체 목록" },
            new TestCase { TestId = "T4-002", TestName = "관리자 권한 검증", Description = "일반 사용자가 관리자 API 접근", PageNumber = 4, ExpectedResult = "403 Forbidden" },
            new TestCase { TestId = "T4-003", TestName = "신고 목록 조회", Description = "관리자가 신고 목록 조회", PageNumber = 4, ExpectedResult = "200 OK, 신고 목록" },
            new TestCase { TestId = "T4-004", TestName = "신고 승인 처리", Description = "신고 승인 및 콘텐츠 처리", PageNumber = 4, ExpectedResult = "200 OK, 상태 변경" },
            new TestCase { TestId = "T4-005", TestName = "신고 거부 처리", Description = "신고 거부", PageNumber = 4, ExpectedResult = "200 OK, 상태 변경" },
            new TestCase { TestId = "T4-006", TestName = "콘텐츠 블라인드", Description = "게시물/댓글 블라인드 처리", PageNumber = 4, ExpectedResult = "200 OK, isBlinded = true" },
            new TestCase { TestId = "T4-007", TestName = "일괄 삭제", Description = "여러 게시물 일괄 삭제", PageNumber = 4, ExpectedResult = "200 OK, 삭제 수 반환" },
            new TestCase { TestId = "T4-008", TestName = "통계 조회", Description = "게시판 통계 데이터", PageNumber = 4, ExpectedResult = "200 OK, 통계 데이터" },
            new TestCase { TestId = "T4-009", TestName = "질문 작성 성공", Description = "Q&A 질문 작성", PageNumber = 4, ExpectedResult = "201 Created" },
            new TestCase { TestId = "T4-010", TestName = "질문 목록 조회", Description = "Q&A 질문 목록", PageNumber = 4, ExpectedResult = "200 OK, 질문 목록" },
            new TestCase { TestId = "T4-011", TestName = "답변 작성 성공", Description = "질문에 답변 작성", PageNumber = 4, ExpectedResult = "201 Created" },
            new TestCase { TestId = "T4-012", TestName = "답변 채택 성공", Description = "질문자가 답변 채택", PageNumber = 4, ExpectedResult = "200 OK, isAccepted = true" },
            new TestCase { TestId = "T4-013", TestName = "답변 채택 - 권한 없음", Description = "질문자 아닌 사용자가 채택 시도", PageNumber = 4, ExpectedResult = "403 Forbidden" },
            new TestCase { TestId = "T4-014", TestName = "답변 추천", Description = "답변 추천", PageNumber = 4, ExpectedResult = "voteCount 증가" },
            new TestCase { TestId = "T4-015", TestName = "질문 상태 변경", Description = "답변 채택 시 질문 상태 자동 변경", PageNumber = 4, ExpectedResult = "status = Answered" }
        };
    }

    /// <summary>
    /// 전체 테스트 상태 조회
    /// </summary>
    public TestStatus GetTestStatus()
    {
        lock (_lock)
        {
            var pageSummaries = new List<PageTestSummary>();

            for (int page = 1; page <= 4; page++)
            {
                var pageTests = _testCases.Where(t => t.PageNumber == page).ToList();
                var pageResults = _testResults.Where(r => r.PageNumber == page).ToList();

                pageSummaries.Add(new PageTestSummary
                {
                    PageNumber = page,
                    PageTitle = PageTitles[page],
                    TotalTests = pageTests.Count,
                    PassedTests = pageResults.Count(r => r.Passed),
                    FailedTests = pageResults.Count(r => !r.Passed),
                    LastExecutedAt = pageResults.OrderByDescending(r => r.ExecutedAt).FirstOrDefault()?.ExecutedAt,
                    TestResults = pageResults
                });
            }

            return new TestStatus
            {
                TotalTests = _testCases.Count,
                PassedTests = _testResults.Count(r => r.Passed),
                FailedTests = _testResults.Count(r => !r.Passed),
                PageSummaries = pageSummaries,
                LastUpdatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// 페이지별 테스트 상태 조회
    /// </summary>
    public PageTestSummary GetPageTestStatus(int pageNumber)
    {
        lock (_lock)
        {
            var pageTests = _testCases.Where(t => t.PageNumber == pageNumber).ToList();
            var pageResults = _testResults.Where(r => r.PageNumber == pageNumber).ToList();

            return new PageTestSummary
            {
                PageNumber = pageNumber,
                PageTitle = PageTitles.GetValueOrDefault(pageNumber, "알 수 없음"),
                TotalTests = pageTests.Count,
                PassedTests = pageResults.Count(r => r.Passed),
                FailedTests = pageResults.Count(r => !r.Passed),
                LastExecutedAt = pageResults.OrderByDescending(r => r.ExecutedAt).FirstOrDefault()?.ExecutedAt,
                TestResults = pageResults
            };
        }
    }

    /// <summary>
    /// 페이지별 테스트 실행
    /// </summary>
    public async Task<List<TestResult>> RunPageTestsAsync(int pageNumber)
    {
        var pageTests = _testCases.Where(t => t.PageNumber == pageNumber).ToList();
        var results = new List<TestResult>();

        foreach (var test in pageTests)
        {
            var result = await RunSingleTestAsync(test);
            results.Add(result);
        }

        lock (_lock)
        {
            // 기존 페이지 결과 제거 후 새 결과 추가
            _testResults.RemoveAll(r => r.PageNumber == pageNumber);
            _testResults.AddRange(results);
        }

        return results;
    }

    /// <summary>
    /// 전체 테스트 실행
    /// </summary>
    public async Task<TestStatus> RunAllTestsAsync()
    {
        var allResults = new List<TestResult>();

        for (int page = 1; page <= 4; page++)
        {
            var pageResults = await RunPageTestsAsync(page);
            allResults.AddRange(pageResults);
        }

        return GetTestStatus();
    }

    /// <summary>
    /// 단일 테스트 실행
    /// 참고: 현재는 시뮬레이션 모드로 동작합니다.
    /// 실제 라이브러리 구현 후 실제 테스트 로직으로 교체해야 합니다.
    /// </summary>
    private async Task<TestResult> RunSingleTestAsync(TestCase testCase)
    {
        var stopwatch = Stopwatch.StartNew();
        var passed = false;
        string? errorMessage = null;

        try
        {
            // TODO: 실제 테스트 로직은 BoardCommonLibrary 구현 후 추가
            // 현재는 시뮬레이션 모드로 동작 (개발/데모 목적)
            // 시뮬레이션 딜레이 (10-100ms)
            await Task.Delay(Random.Shared.Next(10, 100));
            
            // 시뮬레이션: 약 80% 성공률로 랜덤 결과 생성
            // 실제 구현 시 이 부분을 실제 API 호출 및 검증 로직으로 교체
            passed = Random.Shared.Next(0, 10) > 2;
            
            if (!passed)
            {
                errorMessage = "테스트가 예상 결과와 일치하지 않습니다. (시뮬레이션 모드)";
            }
        }
        catch (Exception ex)
        {
            passed = false;
            errorMessage = ex.Message;
        }

        stopwatch.Stop();

        return new TestResult
        {
            TestId = testCase.TestId,
            TestName = testCase.TestName,
            PageNumber = testCase.PageNumber,
            Passed = passed,
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
            ExecutedAt = DateTime.UtcNow,
            ErrorMessage = errorMessage,
            Details = testCase.Description
        };
    }

    /// <summary>
    /// 테스트 케이스 목록 조회
    /// </summary>
    public List<TestCase> GetTestCases(int? pageNumber = null)
    {
        if (pageNumber.HasValue)
        {
            return _testCases.Where(t => t.PageNumber == pageNumber.Value).ToList();
        }
        return _testCases;
    }
}
