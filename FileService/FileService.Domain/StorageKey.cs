using CSharpFunctionalExtensions;
using SharedKernel;
using System.Text.Json.Serialization;

namespace FileService.Domain;

public sealed record StorageKey
{
    public string Key { get; init; } = null!;

    public string? Prefix { get; init; }

    public string Bucket { get; init; } = null!;

    public string Value { get; init; } = null!;

    public string FullPath { get; init; } = null!;

    [JsonConstructor]
    private StorageKey(string bucket, string prefix, string key)
    {
        Bucket = bucket;
        Prefix = prefix;
        Key = key;
        Value = string.IsNullOrEmpty(Prefix) ? Key : $"{Prefix}/{Key}";
        FullPath = $"{Bucket}/{Value}";
    }

    public static Result<StorageKey, Error> Create(string bucket, string? prefix, string key)
    {
        if (string.IsNullOrEmpty(bucket))
        {
            return GeneralErrors.ValueIsInvalid("location");
        }

        Result<string, Error> normalizedKeyResult = NormalizeSegment(key);
        if (normalizedKeyResult.IsFailure)
        {
            return normalizedKeyResult.Error;
        }

        Result<string, Error> normalizedPrefixResult = NormalizePrefix(prefix);
        if (normalizedPrefixResult.IsFailure)
        {
            return normalizedPrefixResult.Error;
        }

        return new StorageKey(bucket.Trim(), normalizedPrefixResult.Value, normalizedKeyResult.Value);
    }

    private static Result<string, Error> NormalizePrefix(string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return string.Empty;

        string[] segments = prefix.Trim().Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        List<string> normalizedSegments = [];
        foreach (string segment in segments)
        {
            Result<string, Error> normalizeSegmentResult = NormalizeSegment(segment);

            if (normalizeSegmentResult.IsFailure)
                return normalizeSegmentResult;

            if (!string.IsNullOrEmpty(normalizeSegmentResult.Value))
                normalizedSegments.Add(normalizeSegmentResult.Value);
        }

        return string.Join("/", normalizedSegments);
    }

    private static Result<string, Error> NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        string trimmed = value.Trim();

        if (trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
            return GeneralErrors.ValueIsInvalid("storageKey.key");

        return trimmed;
    }
}