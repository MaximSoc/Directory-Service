using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Shared;

namespace DirectoryService.Domain.ValueObjects.DepartmentVO
{
    public record DepartmentIdentifier
    {
        private const int MIN_LENGTH = 3;

        private const int MAX_LENGTH = 150;
        public string Value { get; }

        public DepartmentIdentifier(string value)
        {
            Value = value;
        }

        public static Result<DepartmentIdentifier, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return GeneralErrors.ValueIsRequired("DepartmentIdentifier");
            }

            if (value.Length < MIN_LENGTH
                || value.Length > MAX_LENGTH
                || !Regex.IsMatch(value, @"^[a-zA-z]+$")
                )
            {
                return GeneralErrors.ValueIsInvalid("DepartmentIdentifier");
            }


            return new DepartmentIdentifier(value);
        }
    }
}
