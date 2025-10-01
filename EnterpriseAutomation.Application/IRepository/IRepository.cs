using EnterpriseAutomation.Domain.Entities.Base;
using EnterpriseAutomation.Infrastructure.Utilities;
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

        public Task<IEnumerable<TEntity?>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            bool asNoTracking = false);

        public Task<List<TEntity>> GetAll(
         Expression<Func<TEntity, bool>>? predicate = null,
         Expression<Func<TEntity, object>>? orderBy = null,
         bool ascending = true,
         int? page = null,
         int? pageSize = null,
         Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null);

        public Task<PaginatedList<TEntity?>> GetAllPaginationAsync(int padeIndex,int pageSize,
            Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
            bool asNoTracking = false);

        public Task<TEntity?> GetFirstWithInclude(Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = false);

        Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity?> GetSingleAsync(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity?>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate);

        public Task<TEntity?> GetByIdAsync(int Id);

        public Task<EntityEntry<TEntity>> InsertAsync(TEntity entity);
        public Task InsertAsync(IEnumerable<TEntity> entities);

        public void UpdateEntity(TEntity entity);
        public void UpdateEntity(IEnumerable<TEntity> entities);


        public void DeleteEntity(TEntity entity);

        public Task<bool> DeleteByIdAsync(int Id);

        public Task SaveChangesAsync();

        IQueryable<TEntity> GetQueryable(
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool asNoTracking = false);

    }
}
