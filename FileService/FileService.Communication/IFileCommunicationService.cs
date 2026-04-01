using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Contracts.MediaAssets.Responses;
using SharedKernel;

namespace FileService.Communication;

public interface IFileCommunicationService
{
    Task<Result<GetMediaAssetDto?, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken ct);

    Task<Result<GetMediaAssetInfoBatchResponse, Errors>> GetMediaAssetsInfo(GetMediaAssetInfoBatchRequest request, CancellationToken ct);
}