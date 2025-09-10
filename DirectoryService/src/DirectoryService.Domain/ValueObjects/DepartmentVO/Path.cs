using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.ValueObjects.DepartmentVO
{
    public record Path
    {
        public string Value { get; }
        private Path(string value)
        {
            Value = value;
        }

        public static Result<Path, string> Create (string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "Path cannot be empty";
            }

            return new Path(value);
        }
    }
}
