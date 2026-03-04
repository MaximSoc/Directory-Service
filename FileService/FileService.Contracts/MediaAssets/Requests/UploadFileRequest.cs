using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Contracts.MediaAssets.Requests;
public record UploadFileRequest(IFormFile File, string Context, Guid EntityId, string AssetType);

