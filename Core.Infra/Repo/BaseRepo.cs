using Core.Domain.Interfaces;
using Core.Infra.DataContexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Core.Infra.Repo
{
    public class BaseRepo<TEntity> : IBaseRepo<TEntity> where TEntity : class
    {
        protected readonly DbContext _dbContext;
        protected readonly DbSet<TEntity> _dbset;

        public BaseRepo(IDbContext dbContext)
        {
            _dbContext = dbContext as DbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _dbset = _dbContext.Set<TEntity>();
        }

        public async Task<TEntity> CreateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await _dbset.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<TEntity>> CreateAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities));

            await _dbset.AddRangeAsync(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _dbset.Update(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<IEnumerable<TEntity>> UpdateAsync(IEnumerable<TEntity> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities));

            _dbset.UpdateRange(entities);
            await _dbContext.SaveChangesAsync();
            return entities;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await _dbset.FindAsync(id);
            if (entity == null) return false;

            _dbset.Remove(entity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = _dbset.Where(predicate);
            int count = entities.Count();

            if (count > 0)
            {
                _dbset.RemoveRange(entities);
                await _dbContext.SaveChangesAsync();
            }
            return count;
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbset.AnyAsync(predicate);
        }

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate = null,
                                                     int pageIndex = 0,
                                                     int pageSize = 10)
        {
            IQueryable<TEntity> query = _dbset;

            if (predicate != null)
                query = query.Where(predicate);

            return await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(Guid id)
        {
            return await _dbset.FindAsync(id);
        }

        public async Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbset.FirstOrDefaultAsync(predicate);
        }

        public async Task<TEntity> GetSingleOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _dbset.SingleOrDefaultAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            return predicate == null ? await _dbset.CountAsync() : await _dbset.CountAsync(predicate);
        }

        public Task<TEntity> AttachAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _dbset.Attach(entity);
            return Task.FromResult(entity);
        }
    }
}
