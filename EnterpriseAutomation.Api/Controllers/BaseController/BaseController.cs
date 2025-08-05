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
    public abstract class BaseController<TEntity, TGetDto, TCreatDto, TUpdateDto, TGetDetailDto>
        : ControllerBase
        where TEntity : BaseEntity
    {
        private readonly IRepository<TEntity> _repository;
        private readonly Func<TEntity, TGetDto> _mapToGetDto;
        private readonly Func<TUpdateDto, TEntity, TEntity> _mapToUpdateDto;
        private readonly Func<TEntity, TGetDetailDto> _mapToGetDetailDto;

        public BaseController(IRepository<TEntity> repository,
            Func<TEntity, TGetDto> mapToGetDto,
            Func<TUpdateDto, TEntity, TEntity> mapToUpdateDto,
            Func<TEntity, TGetDetailDto> mapToGetDetailDto)
        {
            _repository = repository;
            _mapToGetDto = mapToGetDto;
            _mapToUpdateDto = mapToUpdateDto;
            _mapToGetDetailDto = mapToGetDetailDto;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<TGetDto>>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();
            var dtos = entities.Select(_mapToGetDto);
            return Ok(dtos);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();
            var result = _mapToGetDetailDto(item);
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
        public async Task<IActionResult> UpdateByIdAsync(int id, TUpdateDto updateDto)
        {
            if (updateDto == null) return NotFound();

            var existEntity = await _repository.GetByIdAsync(id);

            if (existEntity == null) return NotFound();

            var updateEntity = _mapToUpdateDto(updateDto, existEntity);

            _repository.UpdateEntityAsync(updateEntity);
            await _repository.SaveChangesAsync();

            return Ok();
        }

    }
}