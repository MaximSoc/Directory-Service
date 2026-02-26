using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Domain
{
    public interface IMediaAssetFactory
    {
        Result<VideoAsset, Error> CreateVideoForUpload(MediaData mediaData, MediaOwner owner);
        Result<PreviewAsset, Error> CreatePreviewForUpload(MediaData mediaData, MediaOwner owner);
    }
}
