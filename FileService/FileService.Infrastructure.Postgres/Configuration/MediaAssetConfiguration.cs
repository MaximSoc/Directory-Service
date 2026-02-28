using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Postgres.Configuration
{
    public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
    {
        public void Configure(EntityTypeBuilder<MediaAsset> builder)
        {
            builder.ToTable("media_assets");

            builder.HasKey(x => x.Id);

            builder.HasDiscriminator(x => x.AssetType)
                .HasValue<VideoAsset>(AssetType.VIDEO)
                .HasValue<PreviewAsset>(AssetType.PREVIEW);

            builder.OwnsOne(m => m.MediaData, mb =>
            {
                mb.ToJson("media_data");

                mb.OwnsOne(md => md.ContentType, cb =>
                {
                    cb.Property(x => x.Category).HasConversion<string>().HasColumnName("category");
                    cb.Property(x => x.Value).HasColumnName("value");
                });

                mb.OwnsOne(md => md.FileName, fb =>
                {
                    fb.Property(x => x.Name).HasColumnName("name");
                    fb.Property(x => x.Extension).HasColumnName("extension");
                });

                mb.Property(md => md.Size).HasColumnName("size");

                mb.Property(md => md.ExpectedChunksCount).HasColumnName("expected_chunks_count");
            });

            builder.Property(x => x.Id).HasColumnName("id");

            builder.Property(x => x.Status).HasConversion<string>().HasColumnName("status");

            builder.Property(x => x.AssetType)
                .HasColumnName("asset_type")
                .HasConversion<string>();

            builder.OwnsOne(x => x.Owner, ob =>
            {
                ob.Property(oe => oe.Context).HasColumnName("context");

                ob.Property(oe => oe.EntityId).HasColumnName("entity_id");
            });

            builder.Property(x => x.CreatedAt).HasColumnName("created_at");

            builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

            builder.Property(x => x.Key)
                .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<StorageKey>(v, (JsonSerializerOptions?)null)!)
                .HasColumnName("key")
                .HasColumnType("jsonb");
        }
    }
}
