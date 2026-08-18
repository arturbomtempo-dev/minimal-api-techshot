namespace TuneTrail.Api.Data.Database.Entities.Base;

public abstract class BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Deleted { get; set; }
}
