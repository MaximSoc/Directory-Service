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
                .IsRequired()
                .HasColumnName("path");
        }
    }
}
