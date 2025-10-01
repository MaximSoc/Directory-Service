﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public static class GeneralErrors
    {
        public static Error ValueIsInvalid(string? name = null)
        {
            var label = name ?? "value";
            return Error.Validation("value.is.invalid", $"{label} is invalid");
        }

        public static Error NotFound(Guid? id = null)
        {
            var forId = id == null ? "" : $"for Id {id}";
            return Error.NorFound("record.not.found", $"record is not found {forId}");
        }

        public static Error ValueIsRequired (string? name = null)
        {
            var label = name == null ? "" : name;
            return Error.Validation("value.is.invalid", $"{label} is required");
        }
    }
}
