using Microsoft.EntityFrameworkCore;

namespace Core.Infra.DataContexts
{
    public interface IDbContext : IDisposable
    {
        DbSet<TEntity> SetEntity<TEntity>() where TEntity : class;
        Task<int> CommitChangesAsync<TEntity>(TEntity entity);
    }
}
