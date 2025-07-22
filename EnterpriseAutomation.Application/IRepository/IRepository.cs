using EnterpriseAutomation.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.IRepository
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {

        public Task<IEnumerable<TEntity>> GetAllAsync();

        public Task<TEntity> GetByIdAsync(int Id);

        public Task<EntityEntry<TEntity>> InsertAsync(TEntity entity);
        public Task InsertAsync(IEnumerable<TEntity> entities);

        public void UpdateEntityAsync(TEntity entity);
        public void UpdateEntityAsync(IEnumerable<TEntity> entities);
       

        public void DeleteEntity(TEntity entity);

        public Task<bool> DeletedEntity(int Id);

    }
}
