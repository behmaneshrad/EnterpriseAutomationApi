//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using EnterpriseAutomation.Application.IRepository;
//using EnterpriseAutomation.Domain.Entities.Base;
//using EnterpriseAutomation.Infrastructure.Persistence;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.ChangeTracking;

//namespace EnterpriseAutomation.Infrastructure.Repository
//{
//    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
//    {
//        private readonly AppDbContext _db;
//        private DbSet<TEntity> _dbSet;
//        public Repository(AppDbContext context)
//        {
//            _db = context;
//            _dbSet = context.Set<TEntity>();
//        }

//        public async Task<bool> DeletedEntity(int Id)
//        {
//            var entity = await GetByIdAsync(Id);
//            if (entity == null)
//            {
//                return false;
//            }
//            DeleteEntity(entity);
//            return true;
//        }

//        public void DeleteEntity(TEntity entity)
//        {
//             _db.Remove(entity);
//        }

//        public async Task<IEnumerable<TEntity>> GetAllAsync()
//        {
//            IQueryable<TEntity> query = _dbSet;

//            return await query.ToListAsync();
//        }

//        public async Task<TEntity> GetByIdAsync(int Id)
//        {
//            return await _dbSet.FindAsync(Id);
//        }

//        public async Task<EntityEntry<TEntity>> InsertAsync(TEntity entity)
//        {
//            return await _dbSet.AddAsync(entity);
//        }

//        public async Task InsertAsync(IEnumerable<TEntity> entities)
//        {
//            await _dbSet.AddRangeAsync(entities);
//        }

//        public void UpdateEntityAsync(TEntity entity)
//        {
//            _dbSet.Attach(entity);
//            _db.Entry(entity).State = EntityState.Modified; 
//            //Alternatively, you can use _dbSet.Update(entity) if you prefer
//        }

//        public void UpdateEntityAsync(IEnumerable<TEntity> entities)
//        {
//            _dbSet.AttachRange(entities);
//            foreach (var entity in entities)
//            {
//                _db.Entry(entity).State = EntityState.Modified;
//            }
//        }
//        //unit of work pattern, repository pattern, entity framework core, c# code snippet,
//        //enterprise automation, infrastructure repository
//    }
//}
