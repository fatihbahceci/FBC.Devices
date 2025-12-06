using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace FBC.Devices.API.DBModels.Repository;

public abstract class EFRepositoryBase<TEntity, TEntityId, TContext>
    : IAsyncRepository<TEntity, TEntityId>/* ,IRepository<TEntity, TEntityId>*/
    where TEntity : Entity<TEntityId, TEntity>
    where TEntityId : IEquatable<TEntityId>
    where TContext : DbContext
{
    protected readonly TContext _context;
    public EFRepositoryBase(TContext context)
    {
        _context = context;
    }

    public IQueryable<TEntity> Query() => _context.Set<TEntity>();

    #region IAsyncRepository Implementation
    public async Task<PaginateResponseModel<TEntity>> GetListAsync(Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (include != null)
            queryable = include(queryable);
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        if (orderBy != null)
            return await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }
    public async Task<PaginateResponseModel<TEntity>> GetListByDynamicAsync(DynamicQuery dynamic, Expression<Func<TEntity, bool>>? predicate = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query().ToDynamic(dynamic);
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (include != null)
            queryable = include(queryable);
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);
        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (include != null)
            queryable = include(queryable);
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        if (withDeleted)
            queryable = queryable.IgnoreQueryFilters();
        if (predicate != null)
            queryable = queryable.Where(predicate);

        return await queryable.AnyAsync(cancellationToken);
    }



    private async Task CheckEntityDataForAsync(EntityOperation operationType, TEntity entity, bool alsoValidate)
    {
        switch (operationType)
        {
            case EntityOperation.Create:
                //entity.StartDate = DateTime.UtcNow;
                entity.CheckDataFor(operationType, alsoValidate, Query());
                entity.CreatedDate = DateTimeOffset.UtcNow;
                break;
            case EntityOperation.Update:
                //entity.StartDate = DateTime.UtcNow;
                entity.CheckDataFor(operationType, alsoValidate, Query());
                entity.UpdatedDate = DateTimeOffset.UtcNow;
                break;
            case EntityOperation.Delete:
                entity.CheckDataFor(operationType, alsoValidate, Query());
                entity.IsDeleted = true;
                entity.DeletedDate = DateTimeOffset.UtcNow;
                break;
        }
    }
    public async Task<ICollection<TEntity>> ApplyOperationRange(EntityOperation entityOperation, ICollection<TEntity> entities, bool alsoValidate, bool deletePermanent = false)
    {
        foreach (var entity in entities)
        {
            switch (entityOperation)
            {
                case EntityOperation.Create:
                    await CheckEntityDataForAsync(EntityOperation.Create, entity, alsoValidate);
                    break;
                case EntityOperation.Update:
                    await CheckEntityDataForAsync(EntityOperation.Update, entity, alsoValidate);
                    break;
                case EntityOperation.Delete:
                    if (!deletePermanent)
                    {
                        await CheckEntityDataForAsync(EntityOperation.Delete, entity, alsoValidate);
                    }
                    break;
            }
        }
        switch (entityOperation)
        {
            case EntityOperation.Create:
                await _context.AddRangeAsync(entities);
                break;
            case EntityOperation.Update:
                _context.UpdateRange(entities);
                break;
            case EntityOperation.Delete:
                if (!deletePermanent)
                {
                    _context.UpdateRange(entities);
                }
                else
                    _context.RemoveRange(entities);
                break;
        }
        await _context.SaveChangesAsync();
        return entities;
    }
    public async Task<TEntity> ApplyOperation(EntityOperation operationType, TEntity entity, bool alsoValidate, bool deletePermanent = false)
    {
        switch (operationType)
        {
            case EntityOperation.Create:
                await CheckEntityDataForAsync(EntityOperation.Create, entity, alsoValidate);
                await _context.AddAsync(entity);
                break;
            case EntityOperation.Update:
                await CheckEntityDataForAsync(EntityOperation.Update, entity, alsoValidate);
                _context.Update(entity);
                break;
            case EntityOperation.Delete:
                if (!deletePermanent)
                {
                    await CheckEntityDataForAsync(EntityOperation.Delete, entity, alsoValidate);
                    _context.Update(entity);
                }
                else
                    _context.Remove(entity);
                break;
        }
        await _context.SaveChangesAsync();
        return entity;
    }
    #endregion
}

public interface IAsyncRepository<TEntity, TEntityId> : IQuery<TEntity>
    where TEntity : Entity<TEntityId, TEntity>
    where TEntityId : IEquatable<TEntityId>

{
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);

    Task<PaginateResponseModel<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<PaginateResponseModel<TEntity>> GetListByDynamicAsync(
        DynamicQuery dynamic,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<bool> AnyAsync(
      Expression<Func<TEntity, bool>>? predicate = null,
      bool withDeleted = false,
      bool enableTracking = true,
      CancellationToken cancellationToken = default
    );

    Task<ICollection<TEntity>> ApplyOperationRange(EntityOperation operationType, ICollection<TEntity> entities, bool alsoValidate, bool deletePermanent = false);
    Task<TEntity> ApplyOperation(EntityOperation operationType, TEntity entity, bool alsoValidate, bool deletePermanent = false);

}

public static class EFRepositoryBaseExtensions
{
    /// <summary>
    /// English: It's registers repositories in the DI container using the <c>IAsyncRepository&lt;TEntity, TEntityId&gt;</c> interface.
    /// Example:
    /// <code>
    /// internal sealed class CreateDeviceHandler(
    ///     ILogger&lt;CreateDeviceHandler&gt; logger,
    ///     IAsyncRepository&lt;Device, long&gt; repo
    /// ) : IRequestHandler&lt;Command, long&gt;
    /// </code>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    private static IServiceCollection RegisterRepositoriesForBaseInterface(this IServiceCollection services, params Assembly[] assemblies)
    {
        var allAssemblies = assemblies.Length > 0 ? assemblies : AppDomain.CurrentDomain.GetAssemblies();
        var repositoryTypes = allAssemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IAsyncRepository<,>))
                .Select(i => new { RepositoryType = t, InterfaceType = i }));
        foreach (var repository in repositoryTypes)
        {
            services.AddScoped(repository.InterfaceType, repository.RepositoryType);
        }
        return services;
    }

    /// <summary>
    /// Registers all non-generic repository interfaces that inherit from IAsyncRepository&lt;,&gt; and their
    /// corresponding implementations with scoped lifetime in the dependency injection container.
    /// </summary>
    /// <remarks>This method scans the specified assemblies for interfaces that inherit from
    /// IAsyncRepository&lt;,&gt; and registers their concrete implementations as scoped services. If multiple
    /// implementations exist for an interface, only the first one found is registered. This method is intended to
    /// simplify repository registration in applications using dependency injection.</remarks>
    /// <param name="services">The IServiceCollection to which the repository services will be added.</param>
    /// <param name="_assemblies">An optional array of assemblies to scan for repository interfaces and implementations. If not specified or
    /// empty, all assemblies loaded in the current application domain are scanned.</param>
    /// <returns>The IServiceCollection instance with the repository services registered.</returns>
    public static IServiceCollection RegisterRepositories(this IServiceCollection services, params Assembly[] _assemblies)
    {
        var target = typeof(IAsyncRepository<,>);
        RegisterRepositoriesForBaseInterface(services, _assemblies);
        var assemblies = _assemblies.Length > 0 ? _assemblies : AppDomain.CurrentDomain.GetAssemblies();
        // Scan all non-abstract, non-interface types from the provided assemblies
        var allTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();

        //All interfaces that are derived from IAsyncRepository<,>
        var repoInterfaces = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.IsInterface &&
                t.IsGenericType == false && //like IDeviceRepository 
                t.GetInterfaces()
                 .Any(i => i.IsGenericType &&
                           i.GetGenericTypeDefinition() == target)
            )
            .ToList();

        foreach (var repoInterface in repoInterfaces)
        {
            var impl = allTypes.FirstOrDefault(c =>
                repoInterface.IsAssignableFrom(c)); //the class that implements the interface. If there are multiple, it takes the first one.

            if (impl != null)
            {
                services.AddScoped(repoInterface, impl);
            }
        }

        return services;
    }
}
