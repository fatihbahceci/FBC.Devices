using FBC.DBRepository;

namespace FBC.Devices.DBModels;

public abstract class EntityBase<TEntity> : Entity<long,TEntity>,
    IEntityHasSoftDeleteFeature, 
    IEntityHasCreatedDate, 
    IEntityHasUpdatedDate, 
    IEntityHasDeletedDate,
    IEntityHasCheckDataFor<TEntity, long>
     where TEntity : EntityBase<TEntity>
{
    public bool IsDeleted { get;set; }
    public DateTime CreatedDateUTC {get; set;}
    public DateTime? UpdatedDateUTC{get; set;}
    public DateTime? DeletedDateUTC { get; set; }
    public abstract void CheckDataFor(EntityOperation operation, bool alsoValidate, IQueryable<TEntity> query);

}
