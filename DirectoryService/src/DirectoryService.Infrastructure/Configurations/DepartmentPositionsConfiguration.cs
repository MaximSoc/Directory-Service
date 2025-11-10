using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentPositionsConfiguration : IEntityTypeConfiguration<DepartmentPosition>
    {
        public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
        {
            builder.ToTable("department_positions");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired()
                .HasColumnName("id");

            builder.Property(x => x.DepartmentId)
                .IsRequired()
                .HasColumnName("department_id");

            builder.Property(x => x.PositionId)
                .IsRequired()
                .HasColumnName("position_id");

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasColumnName("is_active");
        }
    }
}
