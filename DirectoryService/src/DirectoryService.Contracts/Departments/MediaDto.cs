using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments;

public record MediaDto
{
    public Guid? Id { get; init; }

    public string? Url { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;
}

