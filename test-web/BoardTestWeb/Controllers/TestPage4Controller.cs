using Microsoft.AspNetCore.Mvc;

namespace BoardTestWeb.Controllers;

/// <summary>
/// 페이지 4 테스트 컨트롤러 - 관리자/Q&A
/// </summary>
[ApiController]
[Route("api/page4")]
public class TestPage4Controller : ControllerBase
{
    #region 관리자 기능

    /// <summary>
    /// 관리자 게시물 목록 조회 테스트
    /// </summary>
    [HttpGet("admin/posts")]
    public IActionResult GetAdminPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var posts = Enumerable.Range(1, pageSize).Select(i => new
        {
            id = (page - 1) * pageSize + i,
            title = $"게시물 {(page - 1) * pageSize + i}",
            authorName = $"사용자{Random.Shared.Next(1, 10)}",
            status = Random.Shared.Next(0, 2) == 0 ? "Published" : "Draft",
            isBlinded = false,
            reportCount = Random.Shared.Next(0, 5),
            createdAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30))
        }).ToList();

        return Ok(new
        {
            data = posts,
            meta = new { page, pageSize, totalCount = 500, totalPages = 25 }
        });
    }

    /// <summary>
    /// 관리자 댓글 목록 조회 테스트
    /// </summary>
    [HttpGet("admin/comments")]
    public IActionResult GetAdminComments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var comments = Enumerable.Range(1, pageSize).Select(i => new
        {
            id = (page - 1) * pageSize + i,
            content = $"댓글 내용 {i}",
            postId = Random.Shared.Next(1, 100),
            authorName = $"사용자{Random.Shared.Next(1, 10)}",
            isBlinded = false,
            reportCount = Random.Shared.Next(0, 3),
            createdAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30))
        }).ToList();

        return Ok(new
        {
            data = comments,
            meta = new { page, pageSize, totalCount = 1000, totalPages = 50 }
        });
    }

    /// <summary>
    /// 신고 목록 조회 테스트
    /// </summary>
    [HttpGet("admin/reports")]
    public IActionResult GetReports([FromQuery] string? status = null)
    {
        var reports = Enumerable.Range(1, 10).Select(i => new
        {
            id = i,
            targetType = i % 2 == 0 ? "Post" : "Comment",
            targetId = Random.Shared.Next(1, 100),
            reason = new[] { "욕설", "스팸", "부적절한 내용", "광고" }[Random.Shared.Next(0, 4)],
            reporterName = $"신고자{i}",
            status = status ?? (i % 3 == 0 ? "Processed" : "Pending"),
            createdAt = DateTime.UtcNow.AddDays(-i)
        }).ToList();

        return Ok(reports);
    }

    /// <summary>
    /// 신고 처리 테스트
    /// </summary>
    [HttpPut("admin/reports/{id}")]
    public IActionResult ProcessReport(int id, [FromBody] ProcessReportRequest request)
    {
        return Ok(new
        {
            id = id,
            status = request.Action,
            processedAt = DateTime.UtcNow,
            processedBy = "관리자"
        });
    }

    /// <summary>
    /// 콘텐츠 블라인드 테스트
    /// </summary>
    [HttpPost("admin/blind")]
    public IActionResult BlindContent([FromBody] BlindRequest request)
    {
        return Ok(new
        {
            targetType = request.TargetType,
            targetId = request.TargetId,
            isBlinded = true,
            blindedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 일괄 삭제 테스트
    /// </summary>
    [HttpPost("admin/batch/delete")]
    public IActionResult BatchDelete([FromBody] BatchDeleteRequest request)
    {
        return Ok(new
        {
            deletedCount = request.Ids.Count,
            targetType = request.TargetType,
            deletedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 통계 조회 테스트
    /// </summary>
    [HttpGet("admin/statistics")]
    public IActionResult GetStatistics()
    {
        return Ok(new
        {
            posts = new
            {
                total = 1234,
                today = 23,
                thisWeek = 156,
                thisMonth = 678
            },
            comments = new
            {
                total = 5678,
                today = 89,
                thisWeek = 456,
                thisMonth = 2345
            },
            users = new
            {
                total = 456,
                activeToday = 78,
                newThisWeek = 12
            },
            reports = new
            {
                pending = 15,
                processedToday = 8,
                totalThisMonth = 45
            }
        });
    }

    #endregion

    #region Q&A 게시판

    /// <summary>
    /// 질문 목록 조회 테스트
    /// </summary>
    [HttpGet("questions")]
    public IActionResult GetQuestions([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var questions = Enumerable.Range(1, pageSize).Select(i => new
        {
            id = (page - 1) * pageSize + i,
            title = $"질문 {(page - 1) * pageSize + i}: 이것은 어떻게 해결하나요?",
            authorName = $"질문자{Random.Shared.Next(1, 10)}",
            status = new[] { "Open", "Answered", "Closed" }[Random.Shared.Next(0, 3)],
            answerCount = Random.Shared.Next(0, 10),
            voteCount = Random.Shared.Next(0, 50),
            viewCount = Random.Shared.Next(10, 500),
            tags = new[] { "C#", "ASP.NET", "게시판" }.Take(Random.Shared.Next(1, 4)).ToArray(),
            createdAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 30))
        }).ToList();

        return Ok(new
        {
            data = questions,
            meta = new { page, pageSize, totalCount = 100, totalPages = 5 }
        });
    }

    /// <summary>
    /// 질문 상세 조회 테스트
    /// </summary>
    [HttpGet("questions/{id}")]
    public IActionResult GetQuestion(int id)
    {
        if (id <= 0)
        {
            return NotFound(new { error = "질문을 찾을 수 없습니다." });
        }

        return Ok(new
        {
            id = id,
            title = $"질문 {id}: 게시판 라이브러리를 어떻게 사용하나요?",
            content = "ASP.NET Core에서 게시판 라이브러리를 사용하려고 합니다. 설정 방법을 알려주세요.",
            authorName = "질문자1",
            status = "Open",
            answerCount = 3,
            voteCount = 15,
            viewCount = 234,
            tags = new[] { "ASP.NET", "게시판", "설정" },
            acceptedAnswerId = (int?)null,
            createdAt = DateTime.UtcNow.AddDays(-5)
        });
    }

    /// <summary>
    /// 질문 작성 테스트
    /// </summary>
    [HttpPost("questions")]
    public IActionResult CreateQuestion([FromBody] CreateQuestionRequest request)
    {
        if (string.IsNullOrEmpty(request.Title))
        {
            return BadRequest(new { error = "제목은 필수입니다." });
        }

        return Created($"/api/questions/{1}", new
        {
            id = 1,
            title = request.Title,
            content = request.Content,
            status = "Open",
            createdAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 질문 수정 테스트
    /// </summary>
    [HttpPut("questions/{id}")]
    public IActionResult UpdateQuestion(int id, [FromBody] UpdateQuestionRequest request)
    {
        return Ok(new
        {
            id = id,
            title = request.Title,
            content = request.Content,
            updatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 질문 삭제 테스트
    /// </summary>
    [HttpDelete("questions/{id}")]
    public IActionResult DeleteQuestion(int id)
    {
        return NoContent();
    }

    /// <summary>
    /// 답변 목록 조회 테스트
    /// </summary>
    [HttpGet("questions/{questionId}/answers")]
    public IActionResult GetAnswers(int questionId)
    {
        var answers = Enumerable.Range(1, 5).Select(i => new
        {
            id = i,
            questionId = questionId,
            content = $"답변 {i}: 이렇게 하시면 됩니다...",
            authorName = $"답변자{i}",
            isAccepted = i == 1,
            voteCount = Random.Shared.Next(0, 30),
            createdAt = DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 5))
        }).OrderByDescending(a => a.isAccepted).ThenByDescending(a => a.voteCount).ToList();

        return Ok(answers);
    }

    /// <summary>
    /// 답변 작성 테스트
    /// </summary>
    [HttpPost("questions/{questionId}/answers")]
    public IActionResult CreateAnswer(int questionId, [FromBody] CreateAnswerRequest request)
    {
        if (string.IsNullOrEmpty(request.Content))
        {
            return BadRequest(new { error = "답변 내용은 필수입니다." });
        }

        return Created($"/api/answers/{1}", new
        {
            id = 1,
            questionId = questionId,
            content = request.Content,
            isAccepted = false,
            voteCount = 0,
            createdAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 답변 수정 테스트
    /// </summary>
    [HttpPut("answers/{id}")]
    public IActionResult UpdateAnswer(int id, [FromBody] UpdateAnswerRequest request)
    {
        return Ok(new
        {
            id = id,
            content = request.Content,
            updatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 답변 삭제 테스트
    /// </summary>
    [HttpDelete("answers/{id}")]
    public IActionResult DeleteAnswer(int id)
    {
        return NoContent();
    }

    /// <summary>
    /// 답변 채택 테스트
    /// </summary>
    [HttpPost("answers/{id}/accept")]
    public IActionResult AcceptAnswer(int id)
    {
        return Ok(new
        {
            id = id,
            isAccepted = true,
            acceptedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// 답변 추천 테스트
    /// </summary>
    [HttpPost("answers/{id}/vote")]
    public IActionResult VoteAnswer(int id, [FromBody] VoteRequest request)
    {
        return Ok(new
        {
            id = id,
            voteType = request.VoteType,
            voteCount = request.VoteType == "up" ? 11 : 9
        });
    }

    /// <summary>
    /// 답변 추천 취소 테스트
    /// </summary>
    [HttpDelete("answers/{id}/vote")]
    public IActionResult UnvoteAnswer(int id)
    {
        return Ok(new { id = id, voteCount = 10 });
    }

    #endregion
}

#region Request Models

public class ProcessReportRequest
{
    public string Action { get; set; } = string.Empty; // "approve" or "reject"
    public string? Note { get; set; }
}

public class BlindRequest
{
    public string TargetType { get; set; } = string.Empty; // "Post" or "Comment"
    public int TargetId { get; set; }
}

public class BatchDeleteRequest
{
    public string TargetType { get; set; } = string.Empty; // "Post" or "Comment"
    public List<int> Ids { get; set; } = new();
}

public class CreateQuestionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
}

public class UpdateQuestionRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
}

public class CreateAnswerRequest
{
    public string Content { get; set; } = string.Empty;
}

public class UpdateAnswerRequest
{
    public string Content { get; set; } = string.Empty;
}

public class VoteRequest
{
    public string VoteType { get; set; } = "up"; // "up" or "down"
}

#endregion
