using CSharpFunctionalExtensions;
using FileService.Domain;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core
{
    public interface IS3Provider
    {
        Task<UnitResult<Error>> UploadFileAsync(StorageKey key, Stream stream, MediaData mediaData, CancellationToken cancellationToken = default);

        Task<Result<string, Error>> DownloadFileAsync(StorageKey key, string tempPath, CancellationToken cancellationToken = default);

        Task<Result<string, Error>> DeleteFileAsync(StorageKey key, CancellationToken cancellationToken = default);

        Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey key, MediaData mediaData, CancellationToken cancellationToken = default);

        Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey key, CancellationToken cancellationToken = default);

        Task<Result<IReadOnlyList<string>, Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> keys, CancellationToken cancellationToken = default);
    }
}
