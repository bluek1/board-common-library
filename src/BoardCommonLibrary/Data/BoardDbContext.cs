using System.Text.Json;
using BoardCommonLibrary.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BoardCommonLibrary.Data;

/// <summary>
/// 게시판 데이터베이스 컨텍스트
/// </summary>
public class BoardDbContext : DbContext
{
    public BoardDbContext(DbContextOptions<BoardDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// 게시물 테이블
    /// </summary>
    public DbSet<Post> Posts => Set<Post>();
    
    /// <summary>
    /// 조회 기록 테이블
    /// </summary>
    public DbSet<ViewRecord> ViewRecords => Set<ViewRecord>();
    
    /// <summary>
    /// 댓글 테이블
    /// </summary>
    public DbSet<Comment> Comments => Set<Comment>();
    
    /// <summary>
    /// 좋아요 테이블
    /// </summary>
    public DbSet<Like> Likes => Set<Like>();
    
    /// <summary>
    /// 북마크 테이블
    /// </summary>
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    
    /// <summary>
    /// 파일 첨부 테이블
    /// </summary>
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();
    
    /// <summary>
    /// Q&A 질문 테이블
    /// </summary>
    public DbSet<Question> Questions => Set<Question>();
    
    /// <summary>
    /// Q&A 답변 테이블
    /// </summary>
    public DbSet<Answer> Answers => Set<Answer>();
    
    /// <summary>
    /// 신고 테이블
    /// </summary>
    public DbSet<Report> Reports => Set<Report>();
    
    /// <summary>
    /// 답변 추천/비추천 테이블
    /// </summary>
    public DbSet<AnswerVote> AnswerVotes => Set<AnswerVote>();
    
    /// <summary>
    /// 질문 추천/비추천 테이블
    /// </summary>
    public DbSet<QuestionVote> QuestionVotes => Set<QuestionVote>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Post 엔티티 설정
        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("Posts");
            
            entity.HasKey(p => p.Id);
            
            entity.Property(p => p.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(p => p.Content)
                .IsRequired();
            
            entity.Property(p => p.Category)
                .HasMaxLength(100);
            
            entity.Property(p => p.AuthorName)
                .HasMaxLength(100);
            
            entity.Property(p => p.Slug)
                .HasMaxLength(250);
            
            // Tags를 JSON으로 저장
            entity.Property(p => p.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");
            
            // ExtendedProperties를 JSON으로 저장
            entity.Property(p => p.ExtendedProperties)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
                .HasColumnType("nvarchar(max)");
            
            // 소프트 삭제 글로벌 필터
            entity.HasQueryFilter(p => !p.IsDeleted);
            
            // 인덱스 설정
            entity.HasIndex(p => p.AuthorId);
            entity.HasIndex(p => p.Category);
            entity.HasIndex(p => p.IsPinned);
            entity.HasIndex(p => p.IsDeleted);
            entity.HasIndex(p => p.CreatedAt);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.IsDraft);
        });
        
        // ViewRecord 엔티티 설정
        modelBuilder.Entity<ViewRecord>(entity =>
        {
            entity.ToTable("ViewRecords");
            
            entity.HasKey(v => v.Id);
            
            entity.Property(v => v.IpAddress)
                .HasMaxLength(50);
            
            // Post와의 관계 설정
            entity.HasOne(v => v.Post)
                .WithMany()
                .HasForeignKey(v => v.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 인덱스 설정
            entity.HasIndex(v => v.PostId);
            entity.HasIndex(v => v.UserId);
            entity.HasIndex(v => v.IpAddress);
            entity.HasIndex(v => v.ViewedAt);
            
            // 복합 인덱스 (중복 체크 최적화)
            entity.HasIndex(v => new { v.PostId, v.UserId, v.ViewedAt });
            entity.HasIndex(v => new { v.PostId, v.IpAddress, v.ViewedAt });
        });
        
        // Comment 엔티티 설정
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            
            entity.HasKey(c => c.Id);
            
            entity.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(2000);
            
            entity.Property(c => c.AuthorName)
                .HasMaxLength(100);
            
            // Post와의 관계 설정
            entity.HasOne(c => c.Post)
                .WithMany()
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 자기 참조 관계 설정 (대댓글)
            entity.HasOne(c => c.Parent)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // 인덱스 설정
            entity.HasIndex(c => c.PostId);
            entity.HasIndex(c => c.AuthorId);
            entity.HasIndex(c => c.ParentId);
            entity.HasIndex(c => c.IsDeleted);
            entity.HasIndex(c => c.CreatedAt);
            entity.HasIndex(c => new { c.PostId, c.IsDeleted, c.CreatedAt });
        });
        
        // Like 엔티티 설정
        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("Likes");
            
            entity.HasKey(l => l.Id);
            
            // Post와의 관계 설정
            entity.HasOne(l => l.Post)
                .WithMany()
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Comment와의 관계 설정
            entity.HasOne(l => l.Comment)
                .WithMany()
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 인덱스 설정
            entity.HasIndex(l => l.UserId);
            entity.HasIndex(l => l.PostId);
            entity.HasIndex(l => l.CommentId);
            
            // 유니크 인덱스 (중복 좋아요 방지)
            entity.HasIndex(l => new { l.UserId, l.PostId })
                .IsUnique()
                .HasFilter("[PostId] IS NOT NULL");
            entity.HasIndex(l => new { l.UserId, l.CommentId })
                .IsUnique()
                .HasFilter("[CommentId] IS NOT NULL");
        });
        
        // Bookmark 엔티티 설정
        modelBuilder.Entity<Bookmark>(entity =>
        {
            entity.ToTable("Bookmarks");
            
            entity.HasKey(b => b.Id);
            
            // Post와의 관계 설정
            entity.HasOne(b => b.Post)
                .WithMany()
                .HasForeignKey(b => b.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 인덱스 설정
            entity.HasIndex(b => b.UserId);
            entity.HasIndex(b => b.PostId);
            entity.HasIndex(b => b.CreatedAt);
            
            // 유니크 인덱스 (중복 북마크 방지)
            entity.HasIndex(b => new { b.UserId, b.PostId }).IsUnique();
        });
        
        // FileAttachment 엔티티 설정
        modelBuilder.Entity<FileAttachment>(entity =>
        {
            entity.ToTable("FileAttachments");
            
            entity.HasKey(f => f.Id);
            
            entity.Property(f => f.FileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(f => f.StoredFileName)
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(f => f.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(f => f.StoragePath)
                .HasMaxLength(500);
            
            entity.Property(f => f.ThumbnailPath)
                .HasMaxLength(500);
            
            entity.Property(f => f.UploaderName)
                .HasMaxLength(100);
            
            // ExtendedProperties를 JSON으로 저장
            entity.Property(f => f.ExtendedProperties)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
                .HasColumnType("nvarchar(max)");
            
            // Post와의 관계 설정
            entity.HasOne(f => f.Post)
                .WithMany()
                .HasForeignKey(f => f.PostId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // 소프트 삭제 글로벌 필터
            entity.HasQueryFilter(f => !f.IsDeleted);
            
            // 인덱스 설정
            entity.HasIndex(f => f.PostId);
            entity.HasIndex(f => f.UploaderId);
            entity.HasIndex(f => f.ContentType);
            entity.HasIndex(f => f.IsDeleted);
            entity.HasIndex(f => f.CreatedAt);
            entity.HasIndex(f => new { f.PostId, f.IsDeleted });
            entity.HasIndex(f => new { f.UploaderId, f.CreatedAt });
        });
        
        // Question 엔티티 설정
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions");
            
            entity.HasKey(q => q.Id);
            
            entity.Property(q => q.Title)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(q => q.Content)
                .IsRequired();
            
            entity.Property(q => q.AuthorName)
                .HasMaxLength(100);
            
            // Tags를 JSON으로 저장
            entity.Property(q => q.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("nvarchar(max)");
            
            // ExtendedProperties를 JSON으로 저장
            entity.Property(q => q.ExtendedProperties)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
                .HasColumnType("nvarchar(max)");
            
            // AcceptedAnswer와의 관계 설정
            entity.HasOne(q => q.AcceptedAnswer)
                .WithMany()
                .HasForeignKey(q => q.AcceptedAnswerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            // 소프트 삭제 글로벌 필터
            entity.HasQueryFilter(q => !q.IsDeleted);
            
            // 인덱스 설정
            entity.HasIndex(q => q.AuthorId);
            entity.HasIndex(q => q.Status);
            entity.HasIndex(q => q.IsDeleted);
            entity.HasIndex(q => q.CreatedAt);
            entity.HasIndex(q => new { q.Status, q.CreatedAt });
        });
        
        // Answer 엔티티 설정
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("Answers");
            
            entity.HasKey(a => a.Id);
            
            entity.Property(a => a.Content)
                .IsRequired();
            
            entity.Property(a => a.AuthorName)
                .HasMaxLength(100);
            
            // ExtendedProperties를 JSON으로 저장
            entity.Property(a => a.ExtendedProperties)
                .HasConversion(
                    v => v == null ? null : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null))
                .HasColumnType("nvarchar(max)");
            
            // Question과의 관계 설정
            entity.HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 소프트 삭제 글로벌 필터
            entity.HasQueryFilter(a => !a.IsDeleted);
            
            // 인덱스 설정
            entity.HasIndex(a => a.QuestionId);
            entity.HasIndex(a => a.AuthorId);
            entity.HasIndex(a => a.IsAccepted);
            entity.HasIndex(a => a.IsDeleted);
            entity.HasIndex(a => new { a.QuestionId, a.IsAccepted });
        });
        
        // Report 엔티티 설정
        modelBuilder.Entity<Report>(entity =>
        {
            entity.ToTable("Reports");
            
            entity.HasKey(r => r.Id);
            
            entity.Property(r => r.ReporterName)
                .HasMaxLength(100);
            
            entity.Property(r => r.Description)
                .HasMaxLength(1000);
            
            entity.Property(r => r.ProcessedByName)
                .HasMaxLength(100);
            
            entity.Property(r => r.ProcessingNote)
                .HasMaxLength(500);
            
            // 인덱스 설정
            entity.HasIndex(r => r.TargetType);
            entity.HasIndex(r => r.TargetId);
            entity.HasIndex(r => r.ReporterId);
            entity.HasIndex(r => r.Status);
            entity.HasIndex(r => r.CreatedAt);
            entity.HasIndex(r => new { r.TargetType, r.TargetId });
            entity.HasIndex(r => new { r.Status, r.CreatedAt });
        });
        
        // AnswerVote 엔티티 설정
        modelBuilder.Entity<AnswerVote>(entity =>
        {
            entity.ToTable("AnswerVotes");
            
            entity.HasKey(v => v.Id);
            
            // Answer와의 관계 설정
            entity.HasOne(v => v.Answer)
                .WithMany(a => a.Votes)
                .HasForeignKey(v => v.AnswerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 인덱스 설정
            entity.HasIndex(v => v.AnswerId);
            entity.HasIndex(v => v.UserId);
            
            // 유니크 인덱스 (중복 투표 방지)
            entity.HasIndex(v => new { v.AnswerId, v.UserId }).IsUnique();
        });
        
        // QuestionVote 엔티티 설정
        modelBuilder.Entity<QuestionVote>(entity =>
        {
            entity.ToTable("QuestionVotes");
            
            entity.HasKey(v => v.Id);
            
            // Question과의 관계 설정
            entity.HasOne(v => v.Question)
                .WithMany(q => q.Votes)
                .HasForeignKey(v => v.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // 인덱스 설정
            entity.HasIndex(v => v.QuestionId);
            entity.HasIndex(v => v.UserId);
            
            // 유니크 인덱스 (중복 투표 방지)
            entity.HasIndex(v => new { v.QuestionId, v.UserId }).IsUnique();
        });
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Entities.Base.IEntity && 
                       (e.State == EntityState.Added || e.State == EntityState.Modified));
        
        foreach (var entry in entries)
        {
            var entity = (Entities.Base.IEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                entity.UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}
