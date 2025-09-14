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
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("locations");

            builder.HasKey(l => l.Id).HasName("location_id");

            builder.Property(l => l.Name)
                .HasConversion(l => l.Value, l => new Domain.ValueObjects.LocationVO.LocationName(l))
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");

            builder.Property(l => l.Address)
                .HasConversion(l => l.Value, l => new Domain.ValueObjects.LocationVO.LocationAddress(l))
                .IsRequired()
                .HasColumnName("address");

            builder.Property(l => l.Timezone)
                .HasConversion(l => l.Value, l => new Domain.ValueObjects.LocationVO.LocationTimeZone(l))
                .IsRequired()
                .HasColumnName("timezone");
        }
    }
}
