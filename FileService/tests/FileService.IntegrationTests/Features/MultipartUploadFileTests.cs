using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.Dtos;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Contracts.MediaAssets.Responses;
using FileService.Core.HttpCommunication;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Infrastructure.Postgres;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileService.IntegrationTests.Features;

public class MultipartUploadFileTests : FileServiceTestBase
{
    private readonly IntegrationTestsWebFactory _factory;

    public MultipartUploadFileTests(IntegrationTestsWebFactory factory)
        : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task MultipartUpload_FullCycle_PersistsMediaFile()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        StartMultipartUploadResponse startMultipartUploadResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        var partETags = await UploadChunks(fileInfo, startMultipartUploadResponse, cancellationToken);

        var result = await CompleteMultipartUpload(startMultipartUploadResponse, partETags, cancellationToken);

        Assert.True(result.IsSuccess);

        await ExecuteInDb(async db =>
        {
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startMultipartUploadResponse.MediaAssetId, cancellationToken);

            Assert.Equal(MediaAsset.MediaStatus.UPLOADED, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);

            IAmazonS3 amazonS3Client = _factory.Services.GetRequiredService<IAmazonS3>();

            var objectResponse = await amazonS3Client.GetObjectAsync(
                mediaAsset.Key.Bucket,
                mediaAsset.Key.Value,
                cancellationToken);

            Assert.Equal(objectResponse.ContentLength, fileInfo.Length);
        });
    }

    [Fact]
    public async Task CompleteMultipartUpload_InvalidETags_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        // Загружаем чанки, но портим ETag
        var realPartETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        var corruptedETags = realPartETags.Select(p => new PartETagDto(p.PartNumber, "invalid-etag-123")).ToList();

        // Act
        var result = await CompleteMultipartUpload(startResponse, corruptedETags, cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task MultipartUpload_Abort_CancelsUploadAndMarksAsFailed()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Начинаем загрузку
        StartMultipartUploadResponse startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        // Act
        // 2. Вызываем эндпоинт отмены
        var abortResult = await AbortMultipartUpload(startResponse.MediaAssetId, startResponse.UploadId, cancellationToken);

        // Assert
        Assert.True(abortResult.IsSuccess);

        // 3. Проверяем БД
        await ExecuteInDb(async db =>
        {
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startResponse.MediaAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaAsset.MediaStatus.FAILED, mediaAsset.Status);
        });

        // 4. Проверяем S3
        // Если мы попытаемся загрузить чанк после Abort, S3 должен вернуть 404 (NoSuchUpload)
        var chunkUrl = startResponse.ChunkUploadUrls.First().UploadUrl;
        var content = new ByteArrayContent(new byte[100]);
        var uploadResponse = await HttpClient.PutAsync(chunkUrl, content, cancellationToken);

        Assert.Equal(System.Net.HttpStatusCode.NotFound, uploadResponse.StatusCode);
    }

    [Fact]
    public async Task AbortMultipartUpload_AfterCompletion_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // Act
        var abortResult = await AbortMultipartUpload(startResponse.MediaAssetId, startResponse.UploadId, cancellationToken);

        // Assert
        Assert.True(abortResult.IsFailure);
    }

    [Fact]
    public async Task DeleteFile_ExistingFile_RemovesFromDbAndS3()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Сначала создаем файл (проходим полный цикл загрузки)
        StartMultipartUploadResponse startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        var completeResult = await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        Assert.True(completeResult.IsSuccess);

        // Act
        // 2. Вызываем удаление
        var deleteResponse = await AppHttpClient.DeleteAsync($"/api/files/delete/{startResponse.MediaAssetId}", cancellationToken);
        var deleteResult = await deleteResponse.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(deleteResult.IsSuccess);

        // 3. Проверяем БД (статус должен быть DELETED)
        StorageKey? storageKey = null;

        await ExecuteInDb(async db =>
        {
            var mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startResponse.MediaAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaAsset.MediaStatus.DELETED, mediaAsset.Status);

            storageKey = mediaAsset.Key;
        });

        // 4. Проверяем S3 (объект должен физически исчезнуть)
        IAmazonS3 amazonS3Client = _factory.Services.GetRequiredService<IAmazonS3>();

        await Assert.ThrowsAsync<NoSuchKeyException>(async () =>
        {
            await amazonS3Client.GetObjectAsync(
                storageKey!.Bucket,
                storageKey.Key,
                cancellationToken);
        });
    }

    [Fact]
    public async Task DeleteFile_AlreadyDeleted_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // Act
        // 1-е удаление (успех)
        await AppHttpClient.DeleteAsync($"/api/files/delete/{startResponse.MediaAssetId}", cancellationToken);

        // 2-е удаление (повторное)
        var secondResponse = await AppHttpClient.DeleteAsync($"/api/files/delete/{startResponse.MediaAssetId}", cancellationToken);
        var result = await secondResponse.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DownloadFile_ExistingFile_ReturnsBucketName()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo originalFile = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Подготавливаем файл (загружаем), чтобы он был в S3
        var startResponse = await StartMultipartUpload(originalFile, cancellationToken);
        var partETags = await UploadChunks(originalFile, startResponse, cancellationToken);
        var completeResult = await CompleteMultipartUpload(startResponse, partETags, cancellationToken);
        Assert.True(completeResult.IsSuccess);

        // Act
        // 2. Вызываем эндпоинт
        string dummyPath = "some/fake/path.mkv";
        var response = await AppHttpClient.GetAsync(
            $"/api/files/download?fileId={startResponse.MediaAssetId}&path={Uri.EscapeDataString(dummyPath)}",
            cancellationToken);

        var downloadResult = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(downloadResult.IsSuccess);

        // Проверяем, что вернулось имя бакета
        Assert.False(string.IsNullOrWhiteSpace(downloadResult.Value));
    }

    [Fact]
    public async Task DownloadFile_NonExistentFile_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        // Генерируем ID, которого точно нет в базе
        Guid nonExistentFileId = Guid.NewGuid();
        string dummyPath = "C:/temp/somefile.txt";

        // Act
        // Вызываем эндпоинт для несуществующего ID
        var response = await AppHttpClient.GetAsync(
            $"/api/files/download?fileId={nonExistentFileId}&path={Uri.EscapeDataString(dummyPath)}",
            cancellationToken);

        var downloadResult = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        // 1. Проверяем, что результат помечен как Failure
        Assert.True(downloadResult.IsFailure);

        // 2. Проверяем код ошибки
        Assert.Contains(downloadResult.Error, e => e.Code.Contains("server.failure"));

        // 3. Убеждаемся, что HTTP статус соответствует ошибке
        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DownloadFile_DeletedStatus_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // Удаляем файл
        await AppHttpClient.DeleteAsync($"/api/files/delete/{startResponse.MediaAssetId}", cancellationToken);

        // Act
        var response = await AppHttpClient.GetAsync(
            $"/api/files/download?fileId={startResponse.MediaAssetId}&path=some-path",
            cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task DownloadFile_WhenFileIsDeleted_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo originalFile = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Загружаем файл полностью
        var startResponse = await StartMultipartUpload(originalFile, cancellationToken);
        var partETags = await UploadChunks(originalFile, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // 2. Удаляем файл
        await AppHttpClient.DeleteAsync($"/api/files/delete/{startResponse.MediaAssetId}", cancellationToken);

        // Act
        // 3. Пытаемся скачать удаленный файл
        var response = await AppHttpClient.GetAsync(
            $"/api/files/download?fileId={startResponse.MediaAssetId}&path=C:/temp/test.mkv",
            cancellationToken);

        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<UnitResult<Errors>> AbortMultipartUpload(
        Guid mediaAssetId,
        string uploadId,
        CancellationToken cancellationToken)
    {
        var abortRequest = new Contracts.MediaAssets.Requests.AbortMultipartUploadRequest(mediaAssetId, uploadId);

        var response = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/abort", abortRequest, cancellationToken);

        return await response.HandleResponseAsync(cancellationToken);
    }

    private async Task<StartMultipartUploadResponse> StartMultipartUpload(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        var request = new StartMultipartUploadRequest(
            fileInfo.Name,
            "video/mp4",
            fileInfo.Length,
            "video",
            "department",
            Guid.NewGuid());

        var startMultipartResponse = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/start", request, cancellationToken);

        var startMultipartResult = await startMultipartResponse.HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);

        Assert.True(startMultipartResult.IsSuccess);
        Assert.NotNull(startMultipartResult.Value.UploadId);

        await ExecuteInDb(async db =>
        {
            MediaAsset? mediaAsset = await db.MediaAssets
                .FirstOrDefaultAsync(m => m.Id == startMultipartResult.Value.MediaAssetId, cancellationToken);

            Assert.Equal(MediaAsset.MediaStatus.UPLOADING, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);
        });

        return startMultipartResult.Value;
    }

    private async Task<IReadOnlyList<PartETagDto>> UploadChunks(
        FileInfo fileInfo,
        StartMultipartUploadResponse startMultipartUploadResponse,
        CancellationToken cancellationToken)
    {
        await using var stream = fileInfo.OpenRead();

        var parts = new List<PartETagDto>();

        foreach (ChunkUploadUrl chunkUploadUrl in startMultipartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            byte[] chunk = new byte[startMultipartUploadResponse.ChunkSize];
            int bytesRead = await stream.ReadAsync(chunk.AsMemory(0, startMultipartUploadResponse.ChunkSize), cancellationToken);
            if (bytesRead == 0)
                break;

            var content = new ByteArrayContent(chunk);

            var response = await HttpClient.PutAsync(chunkUploadUrl.UploadUrl, content, cancellationToken);

            var eTag = response.Headers.ETag?.Tag.Trim('"');

            parts.Add(new PartETagDto(chunkUploadUrl.PartNumber, eTag!));
        }

        return parts;
    }

    private async Task<UnitResult<Errors>> CompleteMultipartUpload(
        StartMultipartUploadResponse startMultipartUploadResponse,
        IEnumerable<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        var completeMultipartRequest = new Contracts.MediaAssets.Requests.CompleteMultipartUploadRequest(
            startMultipartUploadResponse.MediaAssetId,
            startMultipartUploadResponse.UploadId,
            partETags.ToList());

        var completeMultipartResponse = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/complete", completeMultipartRequest, cancellationToken);

        var completeMultipartResult = await completeMultipartResponse.HandleResponseAsync(cancellationToken);

        return completeMultipartResult;
    }

    [Fact]
    public async Task GenerateDownloadUrl_ExistingFile_ReturnsValidPresignedUrl()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Подготавливаем файл в S3
        StartMultipartUploadResponse startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // Act
        // 2. Вызываем эндпоинт генерации ссылки
        var response = await AppHttpClient.GetAsync($"/api/files/{startResponse.MediaAssetId}/download-url", cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        // Проверяем, что это URL и он содержит необходимые параметры подписи S3
        Assert.Contains("http", result.Value);
        Assert.Contains("X-Amz-Signature", result.Value);
    }

    [Fact]
    public async Task GenerateDownloadUrl_NonExistentFile_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Guid nonExistentId = Guid.NewGuid();

        // Act
        var response = await AppHttpClient.GetAsync($"/api/files/{nonExistentId}/download-url", cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GenerateDownloadUrl_WhenFileIsStillUploading_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Начинаем загрузку (статус будет UPLOADING)
        StartMultipartUploadResponse startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        // Act
        // 2. Пытаемся получить ссылку до завершения загрузки
        var response = await AppHttpClient.GetAsync($"/api/files/{startResponse.MediaAssetId}/download-url", cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        // 3. Теперь тест должен вернуть IsFailure
        Assert.True(result.IsFailure);

        // 4. Проверяем в БД, что статус действительно не UPLOADED
        await ExecuteInDb(async db =>
        {
            var asset = await db.MediaAssets.FindAsync(startResponse.MediaAssetId);
            Assert.NotEqual(MediaAsset.MediaStatus.UPLOADED, asset?.Status);
        });
    }

    [Fact]
    public async Task GenerateDownloadUrls_MultipleFiles_ReturnsAllUrls()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // Загружаем 2 файла
        var file1 = await StartMultipartUpload(fileInfo, cancellationToken);
        await CompleteMultipartUpload(file1, await UploadChunks(fileInfo, file1, cancellationToken), cancellationToken);

        var file2 = await StartMultipartUpload(fileInfo, cancellationToken);
        await CompleteMultipartUpload(file2, await UploadChunks(fileInfo, file2, cancellationToken), cancellationToken);

        var fileIds = new[] { file1.MediaAssetId, file2.MediaAssetId };

        // Act
        // Передаем массив ID через Query String: ?fileIds=id1&fileIds=id2
        var queryString = string.Join("&", fileIds.Select(id => $"fileIds={id}"));
        var response = await AppHttpClient.GetAsync($"/api/files/download-urls?{queryString}", cancellationToken);
        var result = await response.HandleResponseAsync<IReadOnlyList<MediaUrl>>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, url => Assert.Contains("http", url.PresignedUrl));
    }

    [Fact]
    public async Task GenerateDownloadUrls_SomeIdsMissing_ReturnsOnlyFoundFiles()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var file1 = await StartMultipartUpload(fileInfo, cancellationToken);
        await CompleteMultipartUpload(file1, await UploadChunks(fileInfo, file1, cancellationToken), cancellationToken);

        var fileIds = new[] { file1.MediaAssetId, Guid.NewGuid() }; // Один реальный, один фейковый

        // Act
        var queryString = string.Join("&", fileIds.Select(id => $"fileIds={id}"));
        var response = await AppHttpClient.GetAsync($"/api/files/download-urls?{queryString}", cancellationToken);
        var result = await response.HandleResponseAsync<IReadOnlyList<MediaUrl>>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value); // Должен вернуться только один найденный файл
    }

    [Fact]
    public async Task GenerateDownloadUrls_MixedStatuses_ReturnsOnlyUploaded()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Готовый файл
        var fileUploaded = await StartMultipartUpload(fileInfo, cancellationToken);
        await CompleteMultipartUpload(fileUploaded, await UploadChunks(fileInfo, fileUploaded, cancellationToken), cancellationToken);

        // 2. Файл в процессе загрузки (UPLOADING)
        var fileUploading = await StartMultipartUpload(fileInfo, cancellationToken);

        var fileIds = new[] { fileUploaded.MediaAssetId, fileUploading.MediaAssetId };

        // Act
        var queryString = string.Join("&", fileIds.Select(id => $"fileIds={id}"));
        var response = await AppHttpClient.GetAsync($"/api/files/download-urls?{queryString}", cancellationToken);
        var result = await response.HandleResponseAsync<IReadOnlyList<MediaUrl>>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value); // Должна быть только 1 ссылка вместо 2
    }

    [Fact]
    public async Task GenerateUploadUrl_ExistingFile_ReturnsValidPresignedUrl()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        // Act
        var response = await AppHttpClient.GetAsync($"/api/files/{startResponse.MediaAssetId}/upload-url", cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Contains("http", result.Value);
        Assert.Contains("X-Amz-Signature", result.Value);
    }

    [Fact]
    public async Task GenerateUploadUrl_AlreadyUploaded_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Доводим файл до статуса UPLOADED
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // Act
        // 2. Пытаемся снова получить ссылку на загрузку того же файла
        var response = await AppHttpClient.GetAsync($"/api/files/{startResponse.MediaAssetId}/upload-url", cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GenerateUploadUrl_NonExistentFile_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        Guid nonExistentId = Guid.NewGuid();

        // Act
        var response = await AppHttpClient.GetAsync($"/api/files/{nonExistentId}/upload-url", cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetChunkUploadUrl_ValidRequest_ReturnsUrl()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        var request = new GetChunckUploadUrlRequest(
            startResponse.MediaAssetId,
            startResponse.UploadId,
            5); // Запрашиваем 5-й чанк

        // Act
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/url", request, cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Contains("http", result.Value);
        Assert.Contains("partNumber=5", result.Value);
        Assert.Contains($"uploadId={startResponse.UploadId}", result.Value);
    }

    [Fact]
    public async Task GetChunkUploadUrl_InvalidPartNumber_ReturnsValidationError()
    {
        // Arrange
        var request = new GetChunckUploadUrlRequest(Guid.NewGuid(), "some-upload-id", 0); // PartNumber должен быть > 0

        // Act
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/url", request);
        var result = await response.HandleResponseAsync<string>();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetChunkUploadUrl_AfterCompletion_ReturnsError()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Загружаем файл полностью
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // 2. Пытаемся получить ссылку на чанк для уже готового файла
        var request = new GetChunckUploadUrlRequest(startResponse.MediaAssetId, startResponse.UploadId, 1);

        // Act
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/multipart/url", request, cancellationToken);
        var result = await response.HandleResponseAsync<string>(cancellationToken);

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetMediaAssetInfo_StatusUploaded_ReturnsDtoWithUrl()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var partETags = await UploadChunks(fileInfo, startResponse, cancellationToken);
        await CompleteMultipartUpload(startResponse, partETags, cancellationToken);

        // Act
        var response = await AppHttpClient.GetAsync($"/api/files/{startResponse.MediaAssetId}", cancellationToken);
        var result = await response.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(startResponse.MediaAssetId, result.Value.Id);
        Assert.NotNull(result.Value.DownloadUrl);
        Assert.Contains("http", result.Value.DownloadUrl);
        Assert.Equal(fileInfo.Name, result.Value.FileInfo.FileName);
    }

    [Fact]
    public async Task GetMediaAssetInfo_StatusUploading_ReturnsDtoWithoutUrl()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Начинаем загрузку (статус UPLOADING)
        var startResponse = await StartMultipartUpload(fileInfo, cancellationToken);

        // Act
        var response = await AppHttpClient.GetAsync($"/api/files/{startResponse.MediaAssetId}", cancellationToken);
        var result = await response.HandleResponseAsync<GetMediaAssetDto>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("uploading", result.Value.Status);
        Assert.Null(result.Value.DownloadUrl);
    }

    [Fact]
    public async Task GetMediaAssetInfo_NonExistentId_ReturnsNotFound()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        var response = await AppHttpClient.GetAsync($"/api/files/{nonExistentId}");
        var result = await response.HandleResponseAsync<GetMediaAssetDto>();

        // Assert
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetMediaAssetBatch_AllReady_ReturnsFullInfo()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // Создаем 2 готовых файла
        var file1 = await CreateReadyFile(fileInfo, cancellationToken);
        var file2 = await CreateReadyFile(fileInfo, cancellationToken);

        var request = new GetMediaAssetInfoBatchRequest(new[] { file1, file2 });

        // Act
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/batch", request, cancellationToken);
        var result = await response.HandleResponseAsync<GetMediaAssetInfoBatchResponse>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.MediaAssets.Count);
        Assert.All(result.Value.MediaAssets, asset => {
            Assert.Equal("uploaded", asset.Status);
            Assert.NotNull(asset.DownloadUrl);
        });
    }

    [Fact]
    public async Task GetMediaAssetBatch_MixedStatuses_ReturnsOnlyUploaded()
    {
        // Arrange
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        // 1. Готовый файл
        var readyId = await CreateReadyFile(fileInfo, cancellationToken);

        // 2. Файл в процессе (UPLOADING)
        var uploadingResponse = await StartMultipartUpload(fileInfo, cancellationToken);
        var uploadingId = uploadingResponse.MediaAssetId;

        // 3. Удаленный файл
        var deletedId = await CreateReadyFile(fileInfo, cancellationToken);
        await AppHttpClient.DeleteAsync($"/api/files/delete/{deletedId}");

        var request = new GetMediaAssetInfoBatchRequest(new[] { readyId, uploadingId, deletedId });

        // Act
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/batch", request, cancellationToken);
        var result = await response.HandleResponseAsync<GetMediaAssetInfoBatchResponse>(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Single(result.Value.MediaAssets);
        Assert.Equal(readyId, result.Value.MediaAssets.First().Id);
    }

    [Fact]
    public async Task GetMediaAssetBatch_EmptyIds_ReturnsValidationError()
    {
        // Arrange
        var request = new GetMediaAssetInfoBatchRequest(Array.Empty<Guid>());

        // Act
        var response = await AppHttpClient.PostAsJsonAsync("/api/files/batch", request);
        var result = await response.HandleResponseAsync<GetMediaAssetInfoBatchResponse>();

        // Assert
        Assert.True(result.IsFailure);
    }

    private async Task<Guid> CreateReadyFile(FileInfo fileInfo, CancellationToken ct)
    {
        var start = await StartMultipartUpload(fileInfo, ct);
        var parts = await UploadChunks(fileInfo, start, ct);
        await CompleteMultipartUpload(start, parts, ct);
        return start.MediaAssetId;
    }
}