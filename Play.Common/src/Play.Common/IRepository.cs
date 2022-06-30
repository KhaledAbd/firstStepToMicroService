
using System.Linq.Expressions;

namespace Play.Common
{
    public interface IRepository<T> where T : IEntity
    {
        public Task<IReadOnlyCollection<T>> GetAllAsync();
        public Task<IReadOnlyCollection<T>> GetAllAsync(Expression<Func<T, bool>> filter);
        public Task<T> GetAsync(Guid id);

        public Task<T> GetAsync(Expression<Func<T, bool>> filter);
        
        public Task CreateAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task RemoveAsync(Guid id);
    }
}