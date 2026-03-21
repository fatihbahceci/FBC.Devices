using FBC.DBRepository;

namespace FBC.Devices.API.Models;

public abstract class APIBaseEntity<TEntity> : Entity<int, TEntity>, IEntityHasCheckDataFor<TEntity, int>, IEntityHasCreatedDate
    where TEntity : Entity<int, TEntity>
{
    public DateTime CreatedDateUTC { get; set; }

    public abstract Task CheckDataForAsync(EntityOperation operation, bool alsoValidate, IQueryable<TEntity> query);
}