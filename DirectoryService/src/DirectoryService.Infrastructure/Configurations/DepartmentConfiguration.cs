using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectoryService.Infrastructure.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("departments");

            builder.Property(d => d.Id)
                .IsRequired()
                .HasColumnName("id");

            builder.HasKey(d => d.Id).HasName("department_id");

            builder.Property(d => d.Name)
                .HasConversion(d => d.Value, d => new Domain.ValueObjects.DepartmentVO.DepartmentName(d))
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("name");

            builder.Property(d => d.Identifier)
                .HasConversion(d => d.Value, d => new Domain.ValueObjects.DepartmentVO.DepartmentIdentifier(d))
                .IsRequired()
                .HasMaxLength(150)
                .HasColumnName("identifier");

            builder.Property(d => d.Path)
                .HasConversion(d => d.Value, d => new Domain.ValueObjects.DepartmentVO.DepartmentPath(d))
                .HasColumnType("ltree")
                .IsRequired()
                .HasColumnName("path");

            builder.HasIndex(d => d.Path)
                .HasMethod("gist")
                .HasDatabaseName("idx_departments_path");

            builder.Property(d => d.ParentId)
                .IsRequired(false)
                .HasColumnName("parent_id");

            builder.Property(d => d.Depth)
                .IsRequired()
                .HasColumnName("depth");

            builder.Property(d => d.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(d => d.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            builder.Property(d => d.DeletedAt)
                .IsRequired(false)
                .HasColumnName("deleted_at");

            builder.HasMany(d => d.ChildrenDepartments)
                .WithOne()
                .IsRequired(false)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(d => d.DepartmentLocations)
                .WithOne()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.DepartmentPositions)
                .WithOne()
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
