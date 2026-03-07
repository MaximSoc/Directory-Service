using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FileService.Core.FilesStorage;
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

    public async Task<Result<string, Error>> StartMultipartUpload(StorageKey key, MediaData mediaData, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest
            {
                BucketName = key.Bucket,
                Key = key.Key,
                ContentType = mediaData.ContentType.Value,
            };

            var result =  await _s3Client.InitiateMultipartUploadAsync(request);

            return result.UploadId;
        }

        catch(Exception ex)
        {
            _logger.LogError(ex, "Error staring multipart upload");

            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunksUploadUrls(
        StorageKey key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<Task<ChunkUploadUrl>> tasks = Enumerable.Range(1, totalChunks)
            .Select(async partNumber =>
            {
                await _requestsSemaphore.WaitAsync(cancellationToken);

                try
                {
                    var request = new GetPreSignedUrlRequest
                    {
                        BucketName = key.Bucket,
                        Key = key.Key,
                        Verb = HttpVerb.PUT,
                        UploadId = uploadId,
                        PartNumber = partNumber,
                        Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                        Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
                    };

                    var url = await _s3Client.GetPreSignedURLAsync(request);

                    return new ChunkUploadUrl(partNumber, url);
                }

                finally
                {
                    _requestsSemaphore.Release();
                }
            });

        ChunkUploadUrl[] results = await Task.WhenAll(tasks);

        return results;
    }

    public async Task<UnitResult<Error>> CompleteMultipartUploadAsync(
        StorageKey key,
        string uploadId,
        List<PartETagDto> partETags,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest
            {
                BucketName = key.Bucket,
                Key = key.Key,
                UploadId = uploadId,
                PartETags = partETags.Select(p => new PartETag{ ETag = p.ETag, PartNumber = p.PartNumber}).ToList(),
            };

            var response = await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }

        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<ChunkUploadUrl, Error>> GenerateChunkUploadUrl(StorageKey key, string uploadId, int partNumber,
        CancellationToken cancellationToken)
    {
        GetPreSignedUrlRequest request = new()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            Verb = HttpVerb.PUT,
            UploadId = uploadId,
            PartNumber = partNumber,
            Expires = DateTime.UtcNow.AddMinutes(_s3Options.UploadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP,
        };

        try
        {
            return new ChunkUploadUrl(partNumber, await _s3Client.GetPreSignedURLAsync(request));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading chunk url");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey key, string uploadId,
        CancellationToken cancellationToken = default)
    {
        var request = new AbortMultipartUploadRequest()
        {
            BucketName = key.Bucket,
            Key = key.Value,
            UploadId = uploadId
        };

        try
        {
            await _s3Client.AbortMultipartUploadAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error abort multipart upload");
            return S3ErrorMapper.ToError(ex);
        }

        return UnitResult.Success<Error>();
    }
}
