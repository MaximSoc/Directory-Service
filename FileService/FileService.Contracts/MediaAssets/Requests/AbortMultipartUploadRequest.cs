using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Contracts.MediaAssets.Requests;

public record AbortMultipartUploadRequest(Guid MediaAssetId, string UploadId);
