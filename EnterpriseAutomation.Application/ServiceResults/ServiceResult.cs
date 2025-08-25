using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Application.ServiceResults
{
    public class ServiceResult<TEntity>
    {
        public int Status { get; set; }
        public string? MessageCode { get; set; }
        public TEntity? Entity { get; set; }
        public string? Message { get; set; }
        public bool Error { get; set; }
        public string[]? Errors { get; set; }
        public bool Warning { get; set; }
        public string[]? Warnings { get; set; }

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
                Errors = errors ?? new[] { message }
            };
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}