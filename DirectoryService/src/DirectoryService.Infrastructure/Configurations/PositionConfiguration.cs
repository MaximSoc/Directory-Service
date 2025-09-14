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
                .IsRequired();

        }
    }
}
