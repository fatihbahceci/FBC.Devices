using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace FBC.Devices.API.DBModels.Repository;


public enum EntityOperation
{
    Add,
    Update,
    Delete
}
public abstract class Entity<TId, TEntity> 
    where TId : IEquatable<TId>
    where TEntity : Entity<TId, TEntity>
{
    [Key]
    public TId Id { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    //public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
    //public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedDate { get; set; }
    //public string? DeletedBy { get; set; }

    public Entity()
    {
        Id = default!;
        CreatedDate = DateTimeOffset.UtcNow;
    }

    public Entity(TId id)
    {
        Id = id;
        CreatedDate = DateTimeOffset.UtcNow;
    }



   
    public abstract void CheckDataFor(EntityOperation operation, bool alsoValidate, IQueryable<TEntity> query);
}

