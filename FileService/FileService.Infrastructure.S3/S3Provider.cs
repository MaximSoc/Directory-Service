using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileService.Infrastructure.S3;

public class S3Provider : IS3Provider
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Provider> _logger;
    private readonly S3Options _s3Options;

    private readonly SemaphoreSlim _requestsSemaphore;

    public S3Provider(IAmazonS3 s3Client, IOptions<S3Options> s3Options, ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestsSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests);
    }

    public async Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(key.Bucket, key.Key, cancellationToken);

            return key.Key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file");

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> DownloadFileAsync(StorageKey key, string tempPath, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectAsync(key.Bucket, key.Key, cancellationToken);

        return response.BucketName;
    }

    public async Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            InputStream = stream,
            ContentType = mediaData.ContentType.Value,
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };

        try
        {
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate download url");

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<Task<string>> tasks = keys
                .Select(async key =>
                {
                    await _requestsSemaphore.WaitAsync();
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = key.Bucket,
                        Key = key.Key,
                        Verb = HttpVerb.GET,
                        Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };

                    try
                    {
                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestsSemaphore.Release();
                    }
                });

            string[] results = await Task.WhenAll(tasks);

            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey key, MediaData mediaData, CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = key.Bucket,
            Key = key.Key,
            ContentType = mediaData.ContentType.Value,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };

        try
        {
            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate upload url");

            return S3ErrorMapper.ToError(ex);
        }
    }
}
