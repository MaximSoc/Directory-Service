using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure.Configurations
{
    public class LocationConfiguration : IEntityTypeConfiguration<Location>
    {
        public void Configure(EntityTypeBuilder<Location> builder)
        {
            builder.ToTable("locations");

            builder.Property(l => l.Id)
                .IsRequired()
                .HasColumnName("id");

            builder.HasKey(l => l.Id).HasName("location_id");

            builder.Property(l => l.Name)
                .HasConversion(l => l.Value, l => new Domain.ValueObjects.LocationVO.LocationName(l))
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnName("name");

            builder.HasIndex(l => l.Name)
                .IsUnique()
                .HasDatabaseName("ux_location_name");

            builder.OwnsOne(l => l.Address, address =>
            {
                address.Property(t => t.Country).IsRequired().HasColumnName("country");
                address.Property(t => t.City).IsRequired().HasColumnName("city");
                address.Property(t => t.Region).IsRequired().HasColumnName("region");
                address.Property(t => t.PostalCode).IsRequired().HasColumnName("postal_code");
                address.Property(t => t.Street).IsRequired().HasColumnName("street");
                address.Property(t => t.ApartamentNumber).IsRequired().HasColumnName("apartament_number");

                address.HasIndex(t => new { t.Country, t.City, t.Region, t.PostalCode, t.Street, t.ApartamentNumber })
                       .IsUnique()
                       .HasDatabaseName("ux_location_address");
            });

            builder.Property(l => l.Timezone)
                .HasConversion(l => l.Value, l => new Domain.ValueObjects.LocationVO.LocationTimeZone(l))
                .IsRequired()
                .HasColumnName("timezone");

            builder.Property(l => l.IsActive)
                .IsRequired()
                .HasColumnName("is_active");

            builder.Property(l => l.CreatedAt)
                .IsRequired()
                .HasColumnName("created_at");

            builder.Property(l => l.UpdatedAt)
                .IsRequired()
                .HasColumnName("updated_at");

            builder.HasMany(l => l.DepartmentLocations)
                .WithOne()
                .HasForeignKey(l => l.LocationId);
        }
    }
}
