using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Responses;
using SharedKernel;
using System.Net.Http.Json;
using System.Text.Json;

namespace FileService.Core.HttpCommunication;

public static class HttpResponseMessageExtensions
{
    public static async Task<Result<TResponse, Errors>> HandleResponseAsync<TResponse>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default) where TResponse : class
    {
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var envelope = await response.Content.ReadFromJsonAsync<Envelope<TResponse>>(options, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return envelope?.ErrorList ?? GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (envelope is null)
            {
                return GeneralErrors.Failure("Error while reading response").ToErrors();
            }

            if (envelope.ErrorList is not null)
            {
                return envelope.ErrorList;
            }

            return envelope.Result!;
        }

        catch (Exception ex)
        {
            return GeneralErrors.Failure(ex.Message).ToErrors();
        }
    }

    public static async Task<UnitResult<Errors>> HandleResponseAsync(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode &&
            (response.Content.Headers.ContentLength == 0 || response.StatusCode == System.Net.HttpStatusCode.NoContent))
        {
            return UnitResult.Success<Errors>();
        }

        try
        {
            var startMultipartResponse = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(startMultipartResponse))
            {
                return response.IsSuccessStatusCode
                    ? UnitResult.Success<Errors>()
                    : GeneralErrors.Failure("Server returned error without body").ToErrors();
            }

            var envelope = JsonSerializer.Deserialize<Envelope>(startMultipartResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (!response.IsSuccessStatusCode)
            {
                return envelope?.ErrorList ?? GeneralErrors.Failure($"Status: {response.StatusCode}").ToErrors();
            }

            if (envelope?.ErrorList is not null)
            {
                return envelope.ErrorList;
            }

            return UnitResult.Success<Errors>();
        }

        catch (Exception ex)
        {
            return GeneralErrors.Failure(ex.Message).ToErrors();
        }
    }
}
