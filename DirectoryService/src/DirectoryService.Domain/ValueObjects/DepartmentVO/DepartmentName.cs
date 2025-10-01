using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.ValueObjects.DepartmentVO
{
    public record DepartmentName
    {
        private const int MIN_LENGTH = 3;

        private const int MAX_LENGTH = 150;
        public string Value { get; }
        public DepartmentName(string value)
        {
            Value = value;
        }

        public static Result<DepartmentName, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return GeneralErrors.ValueIsRequired("DepartmentName");
            }

            if (value.Length < MIN_LENGTH || value.Length > MAX_LENGTH)
            {
                return GeneralErrors.ValueIsInvalid("DepartmentName");
            }

            return new DepartmentName(value);
        }
    }
}
