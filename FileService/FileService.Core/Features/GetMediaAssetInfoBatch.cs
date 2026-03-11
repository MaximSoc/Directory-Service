using Core.Handlers;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Contracts.MediaAssets.Responses;
using FileService.Core.FilesStorage;
using FileService.Core.Models;
using FileService.Domain;
using FluentValidation;
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

public sealed class GetMediaAssetInfoBatchEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetInfoBatchResponse>> (
                [FromBody] GetMediaAssetInfoBatchRequest request,
                [FromServices] GetMediaAssetInfoBatchHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new GetMediaAssetInfoBatchQuery(request), cancellationToken));
    }
}

public sealed class GetMediaAssetInfoBatchValidator : AbstractValidator<GetMediaAssetInfoBatchQuery>
{
    public GetMediaAssetInfoBatchValidator()
    {
        RuleFor(x => x.Request.MediaAssetIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("mediaAssetIds"));
    }
}

public sealed record GetMediaAssetInfoBatchQuery(GetMediaAssetInfoBatchRequest Request) : IQuery;

public sealed class GetMediaAssetInfoBatchHandler : IQueryHandler<GetMediaAssetInfoBatchResponse, GetMediaAssetInfoBatchQuery>
{
    private readonly IS3Provider _s3Provider;
    private readonly IReadDbContext _readDbContext;
    private readonly IValidator<GetMediaAssetInfoBatchQuery> _validator;

    public GetMediaAssetInfoBatchHandler(
        IS3Provider s3Provider,
        IReadDbContext readDbContext,
        IValidator<GetMediaAssetInfoBatchQuery> validator)
    {
        _s3Provider = s3Provider;
        _readDbContext = readDbContext;
        _validator = validator;
    }

    public async Task<Result<GetMediaAssetInfoBatchResponse, Errors>> Handle(
        GetMediaAssetInfoBatchQuery query,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToList();

        List<MediaAsset> mediaAssets = await _readDbContext.MediaAssetsRead
            .Where(m => query.Request.MediaAssetIds.Contains(m.Id) && m.Status != MediaAsset.MediaStatus.DELETED)
            .ToListAsync();

        var readyMediaAssets = mediaAssets.Where(m => m.Status == MediaAsset.MediaStatus.READY).ToList();

        List<StorageKey> keys = mediaAssets.Select(m =>  m.Key).ToList();

        var urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
        if (urlsResult.IsFailure)
            return urlsResult.Error.ToErrors();

        var urlsDict = urlsResult.Value.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);

        var results = new List<GetMediaAssetsDto>();

        foreach (MediaAsset mediaAsset in readyMediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.Key, out string? url);

            var mediaAssetDto = new GetMediaAssetsDto(
                mediaAsset.Id,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                url);

            results.Add(mediaAssetDto);
        }

        return new GetMediaAssetInfoBatchResponse(results);
    }
}
