using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Repositories
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<LocationsRepository> _logger;

        public LocationsRepository(DirectoryServiceDbContext dbContext, ILogger<LocationsRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Guid> Add(Location location, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Locations.AddAsync(location, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return location.Id;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to insert location");

                // Заменить на Error
                return Guid.Empty;
            }
        }
    }
}
