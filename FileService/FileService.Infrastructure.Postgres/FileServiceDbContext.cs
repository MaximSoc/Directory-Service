using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Postgres
{
    public class FileServiceDbContext : DbContext
    {
        public FileServiceDbContext(DbContextOptions<FileServiceDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FileServiceDbContext).Assembly);
        }

        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

        public IQueryable<MediaAsset> MediaAssetsRead => Set<MediaAsset>().AsQueryable().AsNoTracking();
    }
}
