using System.Linq.Expressions;
using CoinApp.Application.Common.Interfaces;
using CoinApp.Application.Common.Results;
using CoinApp.Domain.Entities;
using CoinApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoinApp.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    protected readonly AppDbContext DbContext;

    protected BaseRepository(AppDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public virtual IQueryable<TEntity> Query() => DbContext.Set<TEntity>().AsNoTracking();

    public virtual Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(TEntity entity)
    {
        DbContext.Set<TEntity>().Update(entity);
    }

    public virtual void Delete(TEntity entity)
    {
        DbContext.Set<TEntity>().Remove(entity);
    }

    public virtual Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return DbContext.Set<TEntity>().AnyAsync(predicate, cancellationToken);
    }

    public virtual async Task<PaginatedResult<TEntity>> PaginateAsync(IQueryable<TEntity> query, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize < 1 ? 10 : pageSize;

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TEntity>(items, safePage, safePageSize, totalCount);
    }
}
