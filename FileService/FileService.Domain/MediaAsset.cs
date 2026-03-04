using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileService.Domain
{
    public abstract partial class MediaAsset
    {
        public Guid Id { get; protected set; }

        public MediaData MediaData { get; protected set; } = null!;

        public AssetType AssetType { get; protected set; }

        public DateTime CreatedAt {  get; protected set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; protected set; }

        public StorageKey Key { get; protected set; } = null!;

        public MediaOwner Owner { get; protected set; } = null!;

        public MediaStatus Status { get; protected set; }

        protected MediaAsset()
        {
        }

        public MediaAsset(
            Guid id,
            MediaData mediaData,
            AssetType assetType,
            MediaStatus status,
            MediaOwner owner,
            StorageKey key)
        {
            Id = id;
            MediaData = mediaData;
            AssetType = assetType;
            Status = status;
            Owner = owner;
            Key = key;
        }

        public static Result<MediaAsset, Error> CreateForUpload(Guid id, MediaData mediaData, AssetType assetType, MediaOwner owner)
        {
            switch (assetType)
            {
                case AssetType.VIDEO:
                    Result<VideoAsset, Error> videoAssetResult = VideoAsset.CreateForUpload(id, mediaData, owner);
                    return videoAssetResult.IsFailure ? videoAssetResult.Error : videoAssetResult.Value;
                case AssetType.PREVIEW:
                    Result<PreviewAsset, Error> previewAssetResult = PreviewAsset.CreateForUpload(id, mediaData, owner);
                    return previewAssetResult.IsFailure ? previewAssetResult.Error : previewAssetResult.Value;
                default: throw new ArgumentOutOfRangeException(nameof(assetType), assetType, null);
            }
        }

        public UnitResult<Error> MarkUploaded()
        {
            if (Status is MediaStatus.UPLOADED)
                return UnitResult.Success<Error>();
            if (Status is not MediaStatus.UPLOADING)
                return Error.Validation(
                    "media.status.invalid",
                    $"Статус медиа файла должен быть: {nameof(MediaStatus.UPLOADING)}!");
            Status = MediaStatus.UPLOADED;
            UpdatedAt = DateTime.UtcNow;
            return UnitResult.Success<Error>();
        }
        public UnitResult<Error> MarkReady(StorageKey key)
        {
            if (Status is MediaStatus.READY)
                return UnitResult.Success<Error>();
            if (Status is not MediaStatus.UPLOADED)
                return Error.Validation(
                    "media.status.invalid",
                    $"Статус медиа файла должен быть: {nameof(MediaStatus.UPLOADED)}!");
            Key = key;
            Status = MediaStatus.READY;
            UpdatedAt = DateTime.UtcNow;
            return UnitResult.Success<Error>();
        }
        public UnitResult<Error> MarkFailed()
        {
            if (Status is MediaStatus.FAILED)
                return UnitResult.Success<Error>();
            if (Status is not (MediaStatus.UPLOADING or MediaStatus.UPLOADED))
                return Error.Validation(
                    "media.status.invalid",
                    $"Статус медиа файла должен быть: {nameof(MediaStatus.UPLOADING)} или {nameof(MediaStatus.UPLOADED)}!");
            Status = MediaStatus.FAILED;
            UpdatedAt = DateTime.UtcNow;
            return UnitResult.Success<Error>();
        }
        public UnitResult<Error> MarkDeleted()
        {
            if (Status is MediaStatus.DELETED)
                return UnitResult.Success<Error>();
            Status = MediaStatus.DELETED;
            UpdatedAt = DateTime.UtcNow;
            return UnitResult.Success<Error>();
        }
    }
}
