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

            builder.ComplexProperty(l => l.Address, tb =>
            {
                tb.Property(t => t.Country)
                .IsRequired()
                .HasColumnName("country");

                tb.Property(t => t.City)
                .IsRequired()
                .HasColumnName("city");

                tb.Property(t => t.Region)
                .IsRequired()
                .HasColumnName("region");

                tb.Property(t => t.PostalCode)
                .IsRequired()
                .HasColumnName("postalCode");

                tb.Property(t => t.Street)
                .IsRequired()
                .HasColumnName("street");

                tb.Property(t => t.ApartamentNumber)
                .IsRequired()
                .HasColumnName("apartamentNumber");

            });

            builder.Property(l => l.Timezone)
                .HasConversion(l => l.Value, l => new Domain.ValueObjects.LocationVO.LocationTimeZone(l))
                .IsRequired()
                .HasColumnName("timezone");
        }
    }
}
