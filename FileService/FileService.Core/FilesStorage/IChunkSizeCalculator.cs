using CSharpFunctionalExtensions;
using SharedKernel;

namespace FileService.Core.FilesStorage;

public interface IChunkSizeCalculator
{
    Result<(long ChunkSize, int TotalChunks), Error> CalculateChunkSize(long fileSize);
}