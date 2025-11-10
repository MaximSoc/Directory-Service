using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace DirectoryService.Infrastructure.Configurations
{
    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.ToTable("positions");

            builder.Property(p => p.Id)
                .IsRequired()
                .HasColumnName("id");

            builder.HasKey(p => p.Id).HasName("position_id");

            builder.Property(p => p.Name)
                .HasConversion(p => p.Value, p => new Domain.ValueObjects.PositionVO.PositionName(p))
                .HasMaxLength(100)
                .HasColumnName("name")
                .IsRequired();

            builder.Property(p => p.Description)
                .HasConversion(p => p.Value, p => new Domain.ValueObjects.PositionVO.PositionDescription(p))
                .HasMaxLength(1000)
                .HasColumnName("description")
                .IsRequired(false);

            builder.Property(p => p.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(p => p.UpdatedAt)
               .IsRequired()
               .HasColumnName("updated_at");

            builder.Property(p => p.DeletedAt)
                .IsRequired(false)
                .HasColumnName("deleted_at");

            builder.HasMany(p => p.DepartmentPositions)
                .WithOne()
                .HasForeignKey(p => p.PositionId);
        }
    }
}
