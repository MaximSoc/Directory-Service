using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared
{
    public record Envelope
    {
        public object? Result { get; }

        public Errors? ErrorList { get; }

        public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());

        public DateTime TimeGenerated { get; }

        [JsonConstructor]
        private Envelope(object? result, Errors? errors)
        {
            Result = result;
            ErrorList = errors;
            TimeGenerated = DateTime.UtcNow;
        }

        public static Envelope Ok(object? result = null) => new(result, null);

        public static Envelope Error(Errors errors) => new(null, errors);
    }

    public record Envelope<T>
    {
        public T? Result { get; }

        public Errors? ErrorList { get; }

        public bool IsError => ErrorList != null || (ErrorList != null && ErrorList.Any());

        public DateTime TimeGenerated { get; }

        [JsonConstructor]
        private Envelope(T? result, Errors? errors)
        {
            Result = result;
            ErrorList = errors;
            TimeGenerated = DateTime.UtcNow;
        }

        public static Envelope<T> Ok(T? result = default) => new(result, null);

        public static Envelope<T> Error(Errors errors) => new(default, errors);
    }
}
