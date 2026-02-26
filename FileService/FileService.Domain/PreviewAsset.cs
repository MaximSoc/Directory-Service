using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Domain
{
    public class PreviewAsset : MediaAsset
    {
        public const long MAX_SIZE = 10_485_760;
        public const string LOCATION = "preview";
        public const string RAW_PREFIX = "raw";
        public const string ALLOWED_CONTENT_TYPE = "image";

        public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];

        public PreviewAsset(
            Guid id,
            MediaData mediaData,
            MediaStatus status,
            MediaOwner owner,
            StorageKey key)
            : base(id, mediaData, AssetType.PREVIEW, status, owner, key)
        {
        }

        public static UnitResult<Error> Validate(MediaData mediaData)
        {
            if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            {
                return Error.Validation("preview.invalid.extension", $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
            }

            if (mediaData.ContentType.Category != MediaType.VIDEO)
            {
                return Error.Validation("preview.invalid.content-type", $"File content type must be {ALLOWED_CONTENT_TYPE}");
            }

            if (mediaData.Size > MAX_SIZE)
            {
                return Error.Validation("preview.invalid.size", $"File size must be less than {MAX_SIZE} bytes");
            }

            return UnitResult.Success<Error>();
        }

        public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
        {
            UnitResult<Error> validationResult = Validate(mediaData);

            if (validationResult.IsFailure)
                return validationResult.Error;

            Result<StorageKey, Error> key = StorageKey.Create(LOCATION, null, id.ToString());
            if (key.IsFailure)
            {
                return key.Error;
            }

            return new PreviewAsset(id, mediaData, MediaStatus.UPLOADING, owner, key.Value);
        }

    }
}
