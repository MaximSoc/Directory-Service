using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects.DepartmentVO;

namespace DirectoryService.Domain.ValueObjects.PositionVO
{
    public record PositionName
    {
        private const int MIN_LENGTH = 3;

        private const int MAX_LENGTH = 100;
        public string Value { get; }
        private PositionName(string value)
        {
            Value = value;
        }

        public static Result<PositionName, string> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                return "PositionName is not correct";
            }

            return new PositionName(value);
        }
    }
}
