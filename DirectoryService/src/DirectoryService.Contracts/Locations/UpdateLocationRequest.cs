using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Locations
{
    public record UpdateLocationRequest(
        Guid LocationId,
        string Name,
        string Country,
        string Region,
        string City,
        string PostalCode,
        string Street,
        string ApartamentNumber,
        string Timezone);
}
