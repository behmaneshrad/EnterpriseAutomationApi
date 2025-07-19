using EnterpriseAutomation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseAutomation.Infrastructure.Persistence.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .IsRequired();

            builder.Property(x => x.CurrentStatus)
                .IsRequired();

            builder.Property(x=>x.CurrentStep)
                .IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(u => u.Requests)
                .HasForeignKey(x=>x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
