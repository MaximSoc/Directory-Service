using CSharpFunctionalExtensions;
using FileService.Contracts.Dtos;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Contracts.MediaAssets.Responses;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System.Net.Http.Json;
using System.Threading;
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

    public async Task<Result<CheckMediaAssetExistsResponse, Errors>> CheckMediaAssetExists(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"files/{mediaAssetId}/exists", cancellationToken);
            var rawContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Request to FileService. Status: {Status}, Body: {Body}",
                response.StatusCode, rawContent);

            return await response.HandleResponseAsync<CheckMediaAssetExistsResponse>(cancellationToken);
        }

        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking media asset for {MediaAssetId}", mediaAssetId);

            return GeneralErrors.Failure("Failed to check media asset exists").ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetDto?, Errors>> GetMediaAssetInfo(Guid mediaAssetId, CancellationToken cancellationToken = default)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"files/{mediaAssetId}", cancellationToken);

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
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                "files/batch",
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("FileService batch request failed. Status: {Status}, Error: {Error}",
                    response.StatusCode, errorContent);
            }

            return (await response.HandleResponseAsync<GetMediaAssetInfoBatchResponse>(cancellationToken))!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for mediaAsset by ids {MediaAssetIds}", string.Join(", ", request.MediaAssetIds));

            return GeneralErrors.Failure("Failed to request media assets info").ToErrors();
        }
    }
}
