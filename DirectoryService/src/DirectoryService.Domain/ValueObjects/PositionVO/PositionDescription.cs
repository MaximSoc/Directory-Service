using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects.PositionVO
{
    public record PositionDescription
    {
        private const int MAX_LENGTH = 1000;

        public string Value { get; }

        public PositionDescription(string value)
        {
            Value = value;
        }

        public static Result<PositionDescription, Error> Create(string value)
        {
            if (value != null && value.Length > MAX_LENGTH)
            {
                return GeneralErrors.ValueIsInvalid("PositionDescription");
            }

            return new PositionDescription(value);
        }
    }
}
