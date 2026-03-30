using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Contracts.MediaAssets.Responses;
using Microsoft.Extensions.Logging;
using SharedKernel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileService.Communication;

internal class FileHttpClient : IFileCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileHttpClient> _logger; 

    public FileHttpClient(HttpClient httpClient, ILogger<FileHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    

    public async Task<Result<GetMediaAssetDto?, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"api/files/{mediaAssetId}", cancellationToken);

            return (await response.HandleResponseAsync<GetMediaAssetDto>(cancellationToken))!;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "Error getting media asset for {MediaAssetId}", mediaAssetId);

            return GeneralErrors.Failure("Failed to request media asset info").ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetInfoBatchResponse, Errors>> GetMediaAssetsInfo(GetMediaAssetInfoBatchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync("api/files/batch", cancellationToken);

            return (await response.HandleResponseAsync<GetMediaAssetInfoBatchResponse>(cancellationToken))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for mediaAsset by ids {MediaAssetIds}", string.Join(", ", request.MediaAssetIds));

            return GeneralErrors.Failure("Failed to request media assets info").ToErrors();
        }
    }
}
