using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.DepartmentVO
{
    public record DepartmentPath
    {
        public string Value { get; }
        public DepartmentPath(string value)
        {
            Value = value;
        }

        public static Result<DepartmentPath, string> Create (string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Path cannot be empty";
            }

            return new DepartmentPath(value);
        }
    }
}
