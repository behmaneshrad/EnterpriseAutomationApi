using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Domain.Entities.Base;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EnterpriseAutomation.Infrastructure.Repository
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly AppDbContext _db;
        private DbSet<TEntity> _dbSet;
        public Repository(AppDbContext context)
        {
            _db = context;
            _dbSet = context.Set<TEntity>();
        }

        public Task DeleteEntity(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            IQueryable<TEntity> query = _dbSet;

            return await query.ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(int Id)
        {
            return await _dbSet.FindAsync(Id);
        }

        public async Task<EntityEntry<TEntity>> InsertAsync(TEntity entity)
        {
            return await _dbSet.AddAsync(entity);
        }

        public async Task InsertAsync(IEnumerable<TEntity> entities)
        {
            await _dbSet.AddRangeAsync(entities);
        }

        public void UpdateEntityAsync(TEntity entity)
        {
            _dbSet.Attach(entity);
            _db.Entry(entity).State = EntityState.Modified;
        }

        public void UpdateEntityAsync(IEnumerable<TEntity> entities)
        {
            _dbSet.AttachRange(entities);
            foreach (var entity in entities)
            {
                _db.Entry(entity).State = EntityState.Modified;
            }
        }
    }
}
