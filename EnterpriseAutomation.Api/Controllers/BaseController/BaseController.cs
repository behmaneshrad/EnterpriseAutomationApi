using Aqua.EnumerableExtensions;
using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.ServiceResult;
using EnterpriseAutomation.Domain.Entities.Base;
using EnterpriseAutomation.Infrastructure.Repository;
using EnterpriseAutomation.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers.BaseController
{
    [Authorize(Policy = "AutoPermission")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TEntity> : ControllerBase
        where TEntity : BaseEntity
    {
        private readonly IRepository<TEntity> _repository;

        public BaseController(IRepository<TEntity> repository)
        {
            _repository = repository;

        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<TEntity>>> GetAllAsync()
        {
            var result = await _repository.GetAllAsync();
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("GetAllPageination/{pageSize}/{pageIndex}")]
        public async Task<ActionResult<ServiceResult<TEntity>>> GetAllWithPaging(int pageIndex = 1, int pageSize = 10)
        {
            var allEntity = await _repository.GetAllAsync();
            if (allEntity == null)
            {
                var result = ServiceResult<TEntity>.Failure("break transiaction", 400);
                return StatusCode(result.Status, result);
            }

            var p = PaginatedList<TEntity>.Create(allEntity.AsQueryable(), pageIndex, pageSize);
            var result1 = ServiceResult<TEntity>.SuccessPaginated(p, 200, "Transient is success");
            return StatusCode(result1.Status, result1);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ServiceResult<TEntity>>> GetById(int id)
        {
            var entity = await _repository.GetByIdAsync(id);

            if (entity == null)
            {
                var result = ServiceResult<TEntity>.Failure("Transient is failure", 404);
                return StatusCode(result.Status, result);
            }
            var res = ServiceResult<TEntity>.Success(entity, 200, "Transient is success");
            return StatusCode(res.Status, res);
        }

        [HttpPost("Add")]
        public async Task<ActionResult<ServiceResult<TEntity>>> AddAsync(TEntity entity)
        {
            try
            {
                await _repository.InsertAsync(entity);
                await _repository.SaveChangesAsync();
                var res = ServiceResult<TEntity>.Success(entity, 201);
                return StatusCode(res.Status, res);
            }
            catch (Exception ex)
            {
                var result = ServiceResult<TEntity>.Failure(ex.Message, 500);
                return StatusCode(result.Status, result);
            }
        }

        [HttpPost("DeleteById/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            var item = await _repository.DeleteByIdAsync(id);
            await _repository.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPost("Update")]
        public async Task<ActionResult<ServiceResult<TEntity>>> UpdateAsync(TEntity updateEntity)
        {
            try
            {
                if (updateEntity == null)
                {
                    var resultFailure = ServiceResult<TEntity>.Failure("Invalid input", 501);
                    return StatusCode(resultFailure.Status, resultFailure);
                }

                _repository.UpdateEntity(updateEntity);
                await _repository.SaveChangesAsync();
                var resultSuccess = ServiceResult<TEntity>.Success(updateEntity, 204);
                return StatusCode(resultSuccess.Status, resultSuccess);
            }
            catch (Exception ex)
            {
                var resultException = ServiceResult<TEntity>.Failure(ex.Message, 500, ex.StackTrace);
                return StatusCode(resultException.Status, resultException);
            }
        }

    }
}