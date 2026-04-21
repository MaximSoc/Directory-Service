using Core.Handlers;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Responses;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

public sealed class CheckMediaAssetExistsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet(
            "/files/{mediaAssetId}/exists", async Task<EndpointResult<CheckMediaAssetExistsResponse>> (
                [FromRoute] Guid mediaAssetId,
                [FromServices] CheckMediaAssetExistsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.Handle(new CheckMediaAssetExistsQuery(mediaAssetId), cancellationToken);

                return result;
            });
    }
}

public sealed record CheckMediaAssetExistsQuery(Guid MediaAssetId) : IQuery;

public sealed class CheckMediaAssetExistsHandler : IQueryHandler<CheckMediaAssetExistsResponse, CheckMediaAssetExistsQuery>
{
    private readonly IReadDbContext _dbContext;

    public CheckMediaAssetExistsHandler(IReadDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<CheckMediaAssetExistsResponse, Errors>> Handle(
        CheckMediaAssetExistsQuery query,
        CancellationToken cancellationToken = new CancellationToken())
    {
        bool exists = await _dbContext.MediaAssetsRead.AnyAsync(x => x.Id == query.MediaAssetId, cancellationToken);

        return new CheckMediaAssetExistsResponse(exists);
    }
}
