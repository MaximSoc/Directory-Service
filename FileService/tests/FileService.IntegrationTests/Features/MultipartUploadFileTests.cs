using FileService.IntegrationTests.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.IntegrationTests.Features;

public class MultipartUploadFileTests : IClassFixture<IntegrationTestsWebFactory>
{
    private readonly IntegrationTestsWebFactory _factory;

    public MultipartUploadFileTests(IntegrationTestsWebFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public void MultipartUpload_FullCycle_PersistsMediaFile()
    {
        var httpClient = _factory.CreateClient();



    }
}
