using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record LocationAddress
    {
        public string Value { get; }
        public LocationAddress(string value)
        {
            Value = value;
        }

        public static Result<LocationAddress, string> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Address cannot be empty";
            }

            return new LocationAddress(value);
        }
    }
}
