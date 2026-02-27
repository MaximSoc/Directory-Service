using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Domain
{
    public enum AssetType
    {
        VIDEO,
        PREVIEW,
        AVATAR
    }

    public static class AssetTypeExtensions
    {
        public static AssetType ToAssetType(this string value)
        {
            return value switch
            {
                "video" => AssetType.VIDEO,
                "preview" => AssetType.PREVIEW,
                "avatar" => AssetType.AVATAR,
                _ => throw new ArgumentException($"Invalid asset type: {value}")
            };
        }
    }
}
