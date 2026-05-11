using System.Linq.Expressions;
using CoinApp.Application.Common.Results;
using CoinApp.Domain.Entities;

namespace CoinApp.Application.Common.Interfaces;

public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> Query();
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<PaginatedResult<TEntity>> PaginateAsync(IQueryable<TEntity> query, int page, int pageSize, CancellationToken cancellationToken = default);
}

