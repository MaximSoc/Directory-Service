using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record LocationName
    {
        private const int MIN_LENGTH = 3;

        private const int MAX_LENGTH = 120;
        public string Value { get; }
        private LocationName(string value)
        {
            Value = value;
        }

        public static Result<LocationName, string> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                return "LocationName is not correct";
            }

            return new LocationName(value);
        }
    }
}
