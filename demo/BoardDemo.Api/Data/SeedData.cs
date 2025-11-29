using BoardCommonLibrary.Data;
using BoardCommonLibrary.Entities;

namespace BoardDemo.Api.Data;

/// <summary>
/// 시드 데이터 생성
/// </summary>
public static class SeedData
{
    // 카테고리 목록 (문자열)
    private static readonly string[] Categories = { "공지사항", "자유게시판", "질문게시판", "정보공유", "잡담" };

    public static async Task InitializeAsync(ApplicationDbContext appContext, BoardDbContext boardContext)
    {
        // 이미 데이터가 있으면 스킵
        if (appContext.Users.Any())
            return;

        // 사용자 생성
        var users = CreateUsers();
        appContext.Users.AddRange(users);
        await appContext.SaveChangesAsync();

        // 게시물 생성
        var posts = CreatePosts(users);
        boardContext.Posts.AddRange(posts);
        await boardContext.SaveChangesAsync();

        // 댓글 생성
        var comments = CreateComments(users, posts);
        boardContext.Comments.AddRange(comments);
        await boardContext.SaveChangesAsync();

        // Q&A 질문 생성
        var questions = CreateQuestions(users);
        boardContext.Questions.AddRange(questions);
        await boardContext.SaveChangesAsync();

        // Q&A 답변 생성
        var answers = CreateAnswers(users, questions);
        boardContext.Answers.AddRange(answers);
        await boardContext.SaveChangesAsync();

        Console.WriteLine("✅ 시드 데이터 생성 완료");
    }

    private static List<Models.AppUser> CreateUsers()
    {
        return new List<Models.AppUser>
        {
            new()
            {
                Username = "admin",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                DisplayName = "관리자",
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                DisplayName = "사용자1",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "user2",
                Email = "user2@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                DisplayName = "사용자2",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "user3",
                Email = "user3@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User123!"),
                DisplayName = "사용자3",
                Role = "User",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Username = "moderator",
                Email = "moderator@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mod123!"),
                DisplayName = "중재자",
                Role = "Moderator",
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    private static List<Post> CreatePosts(List<Models.AppUser> users)
    {
        var posts = new List<Post>();
        var random = new Random(42);

        var sampleTitles = new[]
        {
            "안녕하세요! 첫 게시물입니다",
            "오늘의 날씨가 좋네요",
            "프로그래밍 관련 질문이 있어요",
            "유용한 정보 공유합니다",
            "새로운 기능 업데이트 안내",
            "이벤트 참여 방법 안내",
            "개발자 모임 후기",
            "좋은 책 추천해주세요",
            "주말 계획 공유",
            "버그 리포트입니다"
        };

        var sampleContents = new[]
        {
            "이것은 테스트 게시물의 내용입니다. 게시판 기능을 테스트하기 위해 작성되었습니다.",
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.",
            "# 마크다운 테스트\n\n- 목록 항목 1\n- 목록 항목 2\n- 목록 항목 3\n\n**굵은 글씨** 와 *기울임* 테스트",
            "여러분의 많은 관심과 참여 부탁드립니다. 질문이 있으시면 댓글로 남겨주세요.",
            "이 게시물은 중요한 정보를 담고 있습니다. 꼭 읽어보시기 바랍니다."
        };

        var tags = new[] { "공지", "질문", "정보", "후기", "추천", "이벤트", "개발", "일상" };

        for (int i = 0; i < 50; i++)
        {
            var user = users[random.Next(users.Count)];
            var category = Categories[random.Next(Categories.Length)];
            var dayOffset = random.Next(30);
            var selectedTags = tags.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();

            posts.Add(new Post
            {
                Title = $"{sampleTitles[random.Next(sampleTitles.Length)]} #{i + 1}",
                Content = sampleContents[random.Next(sampleContents.Length)],
                AuthorId = user.Id,
                AuthorName = user.DisplayName ?? user.Username,
                Category = category,
                Status = PostStatus.Published,
                ViewCount = random.Next(0, 500),
                LikeCount = random.Next(0, 50),
                CommentCount = 0,
                IsPinned = i < 3, // 처음 3개는 상단고정
                Tags = selectedTags,
                CreatedAt = DateTime.UtcNow.AddDays(-dayOffset).AddHours(-random.Next(24)),
                PublishedAt = DateTime.UtcNow.AddDays(-dayOffset).AddHours(-random.Next(24))
            });
        }

        return posts;
    }

    private static List<Comment> CreateComments(List<Models.AppUser> users, List<Post> posts)
    {
        var comments = new List<Comment>();
        var random = new Random(42);

        var sampleComments = new[]
        {
            "좋은 글 감사합니다!",
            "저도 같은 생각입니다.",
            "질문이 있어요. 이 부분은 어떻게 하나요?",
            "정보 감사합니다.",
            "도움이 많이 됐어요!",
            "공감합니다~",
            "다음 글도 기대할게요!",
            "이건 처음 알았네요!",
            "좋은 정보네요. 공유해주셔서 감사합니다.",
            "저도 해봐야겠어요."
        };

        foreach (var post in posts.Take(30)) // 30개 게시물에만 댓글
        {
            var commentCount = random.Next(1, 8);
            for (int i = 0; i < commentCount; i++)
            {
                var user = users[random.Next(users.Count)];
                
                comments.Add(new Comment
                {
                    Content = sampleComments[random.Next(sampleComments.Length)],
                    PostId = post.Id,
                    AuthorId = user.Id,
                    AuthorName = user.DisplayName ?? user.Username,
                    LikeCount = random.Next(0, 20),
                    CreatedAt = post.CreatedAt.AddMinutes(random.Next(60, 10000))
                });
            }
        }

        return comments;
    }

    private static List<Question> CreateQuestions(List<Models.AppUser> users)
    {
        var questions = new List<Question>();
        var random = new Random(42);

        var sampleQuestions = new[]
        {
            ("ASP.NET Core에서 JWT 인증 구현하는 방법?", "JWT 토큰을 사용한 인증을 구현하려고 합니다. 어떻게 시작해야 할까요?"),
            ("Entity Framework Core 성능 최적화 팁", "EF Core를 사용하고 있는데 쿼리 성능이 느립니다. 최적화 방법이 있을까요?"),
            ("React vs Vue 어떤 것을 선택해야 할까요?", "새 프로젝트를 시작하는데 프론트엔드 프레임워크 선택에 고민이 있습니다."),
            ("Docker 컨테이너 배포 관련 질문", "Docker를 처음 사용하는데 컨테이너 배포 과정이 궁금합니다."),
            ("C# 비동기 프로그래밍 베스트 프랙티스", "async/await를 사용할 때 주의할 점이 무엇인가요?"),
            ("데이터베이스 인덱스 설계 질문", "어떤 컬럼에 인덱스를 걸어야 효율적일까요?"),
            ("API 설계 RESTful vs GraphQL", "새 API를 설계하는데 어떤 방식이 좋을까요?"),
            ("Git 브랜치 전략 추천", "팀에서 사용하기 좋은 Git 브랜치 전략이 있을까요?"),
            ("로깅 시스템 구축 방법", "효율적인 로깅 시스템을 구축하려고 합니다."),
            ("테스트 코드 작성 팁", "단위 테스트를 처음 작성하는데 어떻게 시작해야 할까요?")
        };

        var tags = new[] { "C#", "ASP.NET", "React", "Docker", "Database", "Git", "Testing", "Performance" };

        for (int i = 0; i < 20; i++)
        {
            var (title, content) = sampleQuestions[i % sampleQuestions.Length];
            var user = users[random.Next(users.Count)];
            var dayOffset = random.Next(30);
            var selectedTags = tags.OrderBy(_ => random.Next()).Take(random.Next(1, 4)).ToList();

            questions.Add(new Question
            {
                Title = $"{title} #{i + 1}",
                Content = content + "\n\n추가 정보나 조언 부탁드립니다.",
                AuthorId = user.Id,
                AuthorName = user.DisplayName ?? user.Username,
                Status = i < 5 ? QuestionStatus.Answered : QuestionStatus.Open,
                ViewCount = random.Next(10, 500),
                VoteCount = random.Next(0, 30),
                AnswerCount = 0,
                Tags = selectedTags,
                CreatedAt = DateTime.UtcNow.AddDays(-dayOffset).AddHours(-random.Next(24))
            });
        }

        return questions;
    }

    private static List<Answer> CreateAnswers(List<Models.AppUser> users, List<Question> questions)
    {
        var answers = new List<Answer>();
        var random = new Random(42);

        var sampleAnswers = new[]
        {
            "해당 문제는 다음과 같이 해결할 수 있습니다.\n\n1. 먼저 설정을 확인하세요.\n2. 그 다음 코드를 수정하세요.\n3. 마지막으로 테스트를 진행하세요.",
            "저도 같은 문제를 겪었는데, 공식 문서를 참고하면 도움이 됩니다. 링크: https://docs.microsoft.com",
            "이 방법을 추천드립니다:\n\n```csharp\n// 예제 코드\nvar result = await SomeMethodAsync();\n```",
            "제 경험상 가장 좋은 방법은 단계별로 접근하는 것입니다.",
            "좋은 질문이네요! 여러 방법이 있지만, 상황에 따라 다르게 적용해야 합니다."
        };

        foreach (var question in questions)
        {
            var answerCount = random.Next(1, 5);
            for (int i = 0; i < answerCount; i++)
            {
                var user = users[random.Next(users.Count)];
                var isAccepted = question.Status == QuestionStatus.Answered && i == 0;

                answers.Add(new Answer
                {
                    Content = sampleAnswers[random.Next(sampleAnswers.Length)],
                    QuestionId = question.Id,
                    AuthorId = user.Id,
                    AuthorName = user.DisplayName ?? user.Username,
                    VoteCount = random.Next(0, 20),
                    IsAccepted = isAccepted,
                    CreatedAt = question.CreatedAt.AddMinutes(random.Next(60, 5000))
                });
            }
        }

        return answers;
    }
}
