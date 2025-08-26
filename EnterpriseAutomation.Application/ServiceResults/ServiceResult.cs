using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.ServiceResult
{
    public class ServiceResult<TEntity>
    {
        public int Status { get; set; }
        public string? MessageCode { get; private set; }
        public TEntity? Entity { get; private set; }
        public string? Message { get; private set; }
        public bool Error { get; private set; }
        public string[]? Errors { get; private set; }
        public bool Warning { get; private set; }
        public string[]? Warnings { get; private set; }
        public IEnumerable<TEntity> Entities { get; private set; } = [];

        public static ServiceResult<TEntity> Success(
           TEntity entity,
           int status = 200,
           string? message = null,
           string? messageCode = null,
           string[]? warnings = null)
        {
            return new ServiceResult<TEntity>
            {
                Status = status,
                Entity = entity,
                Message = message,
                MessageCode = messageCode,
                Warnings = warnings
            };
        }

        public static ServiceResult<TEntity> SuccessList(
        IEnumerable<TEntity> entities,
        int status = 200,
        string? message = null,
        string? messageCode = null,
        string[]? warnings = null)
        {
            return new ServiceResult<TEntity>
            {
                Status = status,
                Entities = entities,
                Message = message,
                MessageCode = messageCode,
                Warnings = warnings
            };
        }

        public static ServiceResult<TEntity> Failure(
           string message,
           int status = 400,
           string? messageCode = null,
           string[]? errors = null)
        {
            return new ServiceResult<TEntity>
            {
                Status = status,
                Message = message,
                MessageCode = messageCode,
                Error = true,
                Errors = errors ?? new[] { message },
                Warning = true
            };
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}