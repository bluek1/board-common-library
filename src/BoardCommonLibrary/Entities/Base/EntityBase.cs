namespace BoardCommonLibrary.Entities.Base;

/// <summary>
/// 모든 엔티티의 기본 인터페이스
/// </summary>
public interface IEntity
{
    long Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 소프트 삭제 지원 인터페이스
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    DateTime? DeletedAt { get; set; }
}

/// <summary>
/// 동적 필드 확장 지원 인터페이스
/// </summary>
public interface IHasExtendedProperties
{
    /// <summary>
    /// 동적 확장 필드. JSON 형식으로 데이터베이스에 저장됩니다.
    /// </summary>
    Dictionary<string, object>? ExtendedProperties { get; set; }
}

/// <summary>
/// 엔티티 기본 클래스
/// </summary>
public abstract class EntityBase : IEntity
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
