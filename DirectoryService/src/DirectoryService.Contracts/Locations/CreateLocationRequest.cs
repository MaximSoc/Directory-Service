using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Locations
{
    public record CreateLocationRequest(LocationNameDto Name, LocationAddressDto Address, LocationTimezoneDto Timezone);
}
