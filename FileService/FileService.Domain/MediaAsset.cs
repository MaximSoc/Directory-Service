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

        private MediaAsset()
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
    }
}
