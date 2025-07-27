using EnterpriseAutomation.Application.IRepository;
using EnterpriseAutomation.Application.Requests.Interfaces;
using EnterpriseAutomation.Domain.Entities;
using EnterpriseAutomation.Domain.Entities.Enums;
using EnterpriseAutomation.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAutomation.Infrastructure.Repository
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _context;
        private readonly IRepository<Request> _repository;

        public RequestRepository(AppDbContext context, IRepository<Request> repository)
        {
            _context = context;
            _repository = repository;
        }

        public async Task<int> CreateAsync(Request request)
        {
            await _repository.InsertAsync(request);
            await _repository.SaveChangesAsync();
            return request.RequestId;
        }

        public async Task<Request?> GetByIdAsync(int id)
        {
            //return await _context.Requests
            //    .Include(r => r.User)
            //    .Include(r => r.ApprovalSteps)
            //        .ThenInclude(a => a.User)
            //    .FirstOrDefaultAsync(r => r.RequestId == id);


            return await _repository.GetFirstWithInclude(
            r => r.RequestId==id,    // یا اگر Generic روی Request ساخته شده: r => r.RequestId == id
            query => query
            .Include(r => r.User)
            .Include(r => r.ApprovalSteps)
            .ThenInclude(a => a.User),
            asNoTracking: true
            );
        }

        public async Task<List<Request>> GetAllAsync()
        {
            //return await _context.Requests
            //    .Include(r => r.User)
            //    .Include(r => r.ApprovalSteps)
            //        .ThenInclude(a => a.User)
            //    .ToListAsync();

            return (List<Request>)await _repository.GetAllAsync(q => q
        .Include(r => r.User)
        .Include(r => r.ApprovalSteps)
            .ThenInclude(a => a.User),
    asNoTracking: true);
        }

        public async Task SubmitAsync(int requestId, int employeeId)
        {
            var request = await GetByIdAsync(requestId);
            if (request != null)
            {
                request.CurrentStatus = RequestStatus.InProgress;
                request.UpdatedAt = DateTime.UtcNow;

                // اضافه کردن مرحله تایید جدید
                var approvalStep = new ApprovalStep
                {
                    RequestId = requestId,
                    StepId = 1, // مرحله اول workflow
                    ApproverUserId = employeeId,
                    Status = ApprovalStatus.Pending,
                    ApprovedAt = null, // هنوز تایید نشده
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.ApprovalSteps.Add(approvalStep);
                _repository.UpdateEntityAsync(request);
                await _repository.SaveChangesAsync();
            }
        }
    }
}