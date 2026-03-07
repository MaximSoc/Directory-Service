using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Contracts.MediaAssets.Requests;

public record StartMultipartUploadRequest(
    string FileName,
    string ContentType,
    long Size,
    string AssetType,
    string Context,
    Guid ContextId);
