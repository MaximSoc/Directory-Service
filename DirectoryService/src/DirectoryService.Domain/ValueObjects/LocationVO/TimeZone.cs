using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NodaTime;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record TimeZone
    {
        public string Value { get; }
        private TimeZone(string value)
        {
            Value = value;
        }

        public static Result<TimeZone, string> Create(string value)
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

            return new TimeZone(value);
        }
    }
}
