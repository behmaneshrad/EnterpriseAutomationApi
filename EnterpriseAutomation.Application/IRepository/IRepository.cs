using EnterpriseAutomation.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.IRepository
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {

        public Task<IEnumerable<TEntity>> GetAllAsync();

        Task<TEntity> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate);

        public Task<TEntity> GetByIdAsync(int Id);

        public Task<EntityEntry<TEntity>> InsertAsync(TEntity entity);
        public Task InsertAsync(IEnumerable<TEntity> entities);

        public void UpdateEntityAsync(TEntity entity);
        public void UpdateEntityAsync(IEnumerable<TEntity> entities);


        public void DeleteEntity(TEntity entity);

        public Task<bool> DeleteByIdAsync(int Id);

        public Task SaveChangesAsync();

    }
}
