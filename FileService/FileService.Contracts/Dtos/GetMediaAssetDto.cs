using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Contracts.Dtos;

public record GetMediaAssetDto
{
    public Guid Id { get; init; }

    public string Status { get; init; } = string.Empty;

    public string AssetType { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }

    public DateTime UpdatedAt { get; init; }

    public required FileInfoDto FileInfo { get; init; }

    public string? DownloadUrl { get; set; }
}
