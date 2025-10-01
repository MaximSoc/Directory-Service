using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record LocationName
    {
        private const int MIN_LENGTH = 3;

        private const int MAX_LENGTH = 120;
        public string Value { get; }
        public LocationName(string value)
        {
            Value = value;
        }

        public static Result<LocationName, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return GeneralErrors.ValueIsRequired("LocationName");
            }

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                return GeneralErrors.ValueIsInvalid("LocationName");
            }

            return new LocationName(value);
        }
    }
}
