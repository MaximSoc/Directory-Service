using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SharedKernel;

namespace DirectoryService.Domain.ValueObjects.DepartmentVO
{
    public record DepartmentPath
    {
        private const char Separator = '.';

        public string Value { get; }

        public DepartmentPath(string value)
        {
            Value = value;
        }

        public static DepartmentPath CreateParent(DepartmentIdentifier identifier)
        {
            return new DepartmentPath(identifier.Value);
        }

        public DepartmentPath CreateChild(DepartmentIdentifier childIdentifier)
        {
            return new DepartmentPath(Value + Separator + childIdentifier.Value);
        }

        public static Result<DepartmentPath, Error> Create (string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return GeneralErrors.ValueIsRequired("DepartmentPath");
            }

            return new DepartmentPath(value);
        }
    }
}
