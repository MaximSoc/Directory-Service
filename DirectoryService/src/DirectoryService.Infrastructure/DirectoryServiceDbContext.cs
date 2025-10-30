using DirectoryService.Application.Database;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure
{
    public class DirectoryServiceDbContext : DbContext, IReadDbContext
    {
        private readonly string _connectionString;

        public DirectoryServiceDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);

            optionsBuilder.EnableDetailedErrors();

            optionsBuilder.EnableSensitiveDataLogging();

            optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresExtension("ltree");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DirectoryServiceDbContext).Assembly);
        }

        public DbSet<Department> Departments => Set<Department>();

        public DbSet<Location> Locations => Set<Location>();

        public DbSet<Position> Positions => Set<Position>();

        public IQueryable<Location> LocationsRead => Set<Location>().AsQueryable().AsNoTracking();

        private ILoggerFactory CreateLoggerFactory() =>
            LoggerFactory.Create(builder => { builder.AddConsole(); });
    }
}
