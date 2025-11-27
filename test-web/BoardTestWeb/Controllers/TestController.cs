using BoardTestWeb.Models;
using BoardTestWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 테스트 API 컨트롤러
/// </summary>
[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly TestExecutionService _testService;

    public TestController(TestExecutionService testService)
    {
        _testService = testService;
    }

    /// <summary>
    /// 전체 테스트 상태 조회
    /// </summary>
    [HttpGet("status")]
    public ActionResult<TestStatus> GetStatus()
    {
        var status = _testService.GetTestStatus();
        return Ok(status);
    }

    /// <summary>
    /// 페이지별 테스트 상태 조회
    /// </summary>
    [HttpGet("page/{pageNumber}")]
    public ActionResult<PageTestSummary> GetPageStatus(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > 4)
        {
            return BadRequest("페이지 번호는 1-4 사이여야 합니다.");
        }

        var status = _testService.GetPageTestStatus(pageNumber);
        return Ok(status);
    }

    /// <summary>
    /// 페이지별 테스트 실행
    /// </summary>
    [HttpPost("run/{pageNumber}")]
    public async Task<ActionResult<List<TestResult>>> RunPageTests(int pageNumber)
    {
        if (pageNumber < 1 || pageNumber > 4)
        {
            return BadRequest("페이지 번호는 1-4 사이여야 합니다.");
        }

        var results = await _testService.RunPageTestsAsync(pageNumber);
        return Ok(results);
    }

    /// <summary>
    /// 전체 테스트 실행
    /// </summary>
    [HttpPost("run/all")]
    public async Task<ActionResult<TestStatus>> RunAllTests()
    {
        var status = await _testService.RunAllTestsAsync();
        return Ok(status);
    }

    /// <summary>
    /// 테스트 케이스 목록 조회
    /// </summary>
    [HttpGet("cases")]
    public ActionResult<List<TestCase>> GetTestCases([FromQuery] int? pageNumber)
    {
        var cases = _testService.GetTestCases(pageNumber);
        return Ok(cases);
    }
}
