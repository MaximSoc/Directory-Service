using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.PositionVO
{
    public record Description
    {
        private const int MAX_LENGTH = 1000;
        public string? Value { get; }
        private Description(string? value)
        {
            Value = value;
        }

        public static Result<Description, string> Create(string? value)
        {
            if (value != null && value.Length > MAX_LENGTH)
            {
                return "Description is not correct";
            }

            return new Description(value);
        }
    }
}
