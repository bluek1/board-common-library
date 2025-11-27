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
