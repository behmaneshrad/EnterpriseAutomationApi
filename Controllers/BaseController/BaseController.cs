using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Domain.Entities.Base;
using EnterpriseAutomation.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers.BaseController
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController<TEntity> : ControllerBase where TEntity : BaseEntity
    {
        private readonly IRepository<TEntity> _repository;

        public BaseController(IRepository<TEntity> repository)
        {
            _repository = repository;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAllAsync();
            return Ok(items);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            return Ok(item);
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddAsync(TEntity entity)
        {
            try
            {
                await _repository.InsertAsync(entity);
                await _repository.SaveChangesAsync();
                return StatusCode(statusCode: 201);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("DeleteById/{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            var item = await _repository.DeleteByIdAsync(id);
            return Ok(item);
        }

    }
}