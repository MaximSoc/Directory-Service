using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.LocationVO
{
    public record Address
    {
        public string Value { get; }
        private Address(string value)
        {
            Value = value;
        }

        public static Result<Address, string> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Address cannot be empty";
            }

            return new Address(value);
        }
    }
}
