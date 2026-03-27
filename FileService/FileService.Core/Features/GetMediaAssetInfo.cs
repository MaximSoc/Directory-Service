using Core.Handlers;
using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Core.FilesStorage;
using FileService.Domain;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core.Features;

public sealed class GetMediaAssetInfoEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/{mediaAssetId:guid}", async Task<EndpointResult<GetMediaAssetDto>> (
                Guid mediaAssetId,
                [FromServices] GetMediaAssetInfoHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new GetMediaAssetInfoQuery(mediaAssetId), cancellationToken));
    }
}

public sealed record GetMediaAssetInfoQuery(Guid MediaAssetId) : IQuery;

public sealed class GetMediaAssetInfoHandler : IQueryHandler<GetMediaAssetDto, GetMediaAssetInfoQuery>
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;

    public GetMediaAssetInfoHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
    }

    public async Task<Result<GetMediaAssetDto, Errors>> Handle(
        GetMediaAssetInfoQuery query,
        CancellationToken cancellationToken = default)
    {
        MediaAsset? mediaAsset = await _readDbContext.MediaAssetsRead
            .FirstOrDefaultAsync( m => m.Id ==  query.MediaAssetId, cancellationToken);

        if (mediaAsset == null)
            return GeneralErrors.NotFound(query.MediaAssetId).ToErrors();

        string? url = null;

        if (mediaAsset.Status == MediaAsset.MediaStatus.UPLOADED)
        {
            var urlResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.Key, cancellationToken);
            if (urlResult.IsFailure)
                return urlResult.Error.ToErrors();

            url = urlResult.Value;
        }

        return new GetMediaAssetDto {
            Id = mediaAsset.Id,
            Status = mediaAsset.Status.ToString().ToLowerInvariant(),
            AssetType = mediaAsset.AssetType.ToString().ToLowerInvariant(),
            CreatedAt = mediaAsset.CreatedAt,
            UpdatedAt = mediaAsset.UpdatedAt,
            FileInfo = new FileInfoDto
            {
                FileName = mediaAsset.MediaData.FileName.Name,
                ContentType = mediaAsset.MediaData.ContentType.Value,
                Size = mediaAsset.MediaData.Size
            },
            DownloadUrl = url };
    }
}
