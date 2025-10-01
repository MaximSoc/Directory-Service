using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using Shared;

namespace DirectoryService.Domain.ValueObjects.PositionVO
{
    public record PositionName
    {
        private const int MIN_LENGTH = 3;

        private const int MAX_LENGTH = 100;
        public string Value { get; }
        public PositionName(string value)
        {
            Value = value;
        }

        public static Result<PositionName, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return GeneralErrors.ValueIsRequired("PositionName");
            }

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                return GeneralErrors.ValueIsInvalid("PositionName");
            }

            return new PositionName(value);
        }
    }
}
