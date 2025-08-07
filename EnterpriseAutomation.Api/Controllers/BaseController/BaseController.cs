using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.WorkflowDefinitions.Models;
using EnterpriseAutomation.Domain.Entities.Base;
using EnterpriseAutomation.Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAutomation.Api.Controllers.BaseController
{
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

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null) return NotFound();
            
            return Ok(result);
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

        [HttpPost("Update/{id}")]
        public async Task<IActionResult> UpdateByIdAsync(TEntity updateEntity)
        {
            if (updateEntity == null) return NotFound();

            _repository.UpdateEntityAsync(updateEntity);
            await _repository.SaveChangesAsync();

            return Ok();
        }

    }
}