using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Contracts.MediaAssets.Responses;

public record CheckMediaAssetExistsResponse(bool Exists);