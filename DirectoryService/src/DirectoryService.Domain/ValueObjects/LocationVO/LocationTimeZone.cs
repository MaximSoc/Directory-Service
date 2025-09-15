using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NodaTime;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record LocationTimeZone
    {
        public string Value { get; }
        public LocationTimeZone(string value)
        {
            Value = value;
        }

        public static Result<LocationTimeZone, string> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "TimeZone cannot be empty";
            }

            var timeZoneDB = DateTimeZoneProviders.Tzdb;
            if (timeZoneDB.GetZoneOrNull(value) == null)
            {
                return "TimeZone is not correct";
            }

            return new LocationTimeZone(value);
        }
    }
}
