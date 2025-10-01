using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NodaTime;
using Shared;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record LocationTimeZone
    {
        public string Value { get; }
        public LocationTimeZone(string value)
        {
            Value = value;
        }

        public static Result<LocationTimeZone, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return GeneralErrors.ValueIsRequired("Timezone");
            }

            var timeZoneDB = DateTimeZoneProviders.Tzdb;
            if (timeZoneDB.GetZoneOrNull(value) == null)
            {
                return GeneralErrors.ValueIsInvalid("Timezone");
            }

            return new LocationTimeZone(value);
        }
    }
}
