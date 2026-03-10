using FileService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core.Models;

public record MediaUrl(StorageKey StorageKey, string PresignedUrl);