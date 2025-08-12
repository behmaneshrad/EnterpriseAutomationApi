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

        public static ServiceResult<TEntity> Success(TEntity entity, int status = 200, string? messageCode = null)
        {
            return new ServiceResult<TEntity>
            {
                Status = status,
                Entity = entity,
                MessageCode = messageCode,
                Error = false
            };
        }

        public static ServiceResult<TEntity> Failure(string[] errors, int status = 400)
        {
            return new ServiceResult<TEntity>
            {
                Status = status,
                Errors = errors,
                Error = true
            };
        }


        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}