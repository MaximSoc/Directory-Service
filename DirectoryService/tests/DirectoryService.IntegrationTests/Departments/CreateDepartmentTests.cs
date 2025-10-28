using DirectoryService.Application.Departments;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using DirectoryService.Domain.ValueObjects.LocationVO;
using DirectoryService.Infrastructure;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace DirectoryService.IntegrationTests.Departments
{
    public class CreateDepartmentTests : DirectoryTestsBase
    {
        public CreateDepartmentTests(DirectoryTestWebFactory factory)
            : base(factory) { }

        [Fact]
        public async Task CreateDepartmentParent_with_valid_data_should_succeed()
        {
            // arrange
            Guid locationId = await CreateLocation("Location");

            var cancellationToken = CancellationToken.None;

            // act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new CreateDepartmentCommand(new Contracts.Departments.CreateDepartmentRequest(
                new Contracts.Departments.DepartmentNameDto("Parent"),
                new Contracts.Departments.DepartmentIdentifierDto("parent"),
                null,
                [locationId]));

                return sut.Handle(command, cancellationToken);
            });


            // assert
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value, cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id, result.Value);

                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);
            });          
        }

        [Fact]
        public async Task CreateDepartmentChildren_with_valid_data_should_succeed()
        {
            // arrange
            Guid locationId = await CreateLocation("Location");

            var cancellationToken = CancellationToken.None;

            Guid parentId = await CreateParent("Parent", locationId);

            // act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new CreateDepartmentCommand(new Contracts.Departments.CreateDepartmentRequest(
                new Contracts.Departments.DepartmentNameDto("Children"),
                new Contracts.Departments.DepartmentIdentifierDto("children"),
                parentId,
                [locationId]));

                return sut.Handle(command, cancellationToken);
            });


            // assert
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(d => d.Id == result.Value, cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id, result.Value);
                Assert.True(department.Depth == 1);
                Assert.True(department.Path.Value != department.Identifier.Value);
                Assert.True(result.IsSuccess);
                Assert.NotEqual(Guid.Empty, result.Value);
            });
        }

        [Fact]
        public async Task CreateDepartment_without_locations_should_failed()
        {
            // arrange
            var cancellationToken = CancellationToken.None;

            // act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new CreateDepartmentCommand(new Contracts.Departments.CreateDepartmentRequest(
                new Contracts.Departments.DepartmentNameDto("Department"),
                new Contracts.Departments.DepartmentIdentifierDto("department"),
                null,
                []));

                return sut.Handle(command, cancellationToken);
            });


            // assert
            Assert.True(result.IsFailure);
            Assert.NotEmpty(result.Error);
        }

        private async Task<Guid> CreateParent(string name, Guid locationId)
        {
            Guid parentId = Guid.NewGuid();
            List<DepartmentLocation> departmentLocations = new List<DepartmentLocation>();
            departmentLocations.Add(new DepartmentLocation(parentId, locationId));

            return await ExecuteInDb(async dbContext =>
            {
                var parent = Department.CreateParent(
                    DepartmentName.Create(name).Value,
                    DepartmentIdentifier.Create("parent").Value,
                    departmentLocations,
                    parentId).Value;

                dbContext.Departments.Add(parent);
                await dbContext.SaveChangesAsync();

                return parent.Id;
            });
        }

        private async Task<Guid> CreateLocation(string name)
        {
            return await ExecuteInDb(async dbContext =>
            {
                var location = new Location(
                    LocationName.Create(name).Value,
                    LocationAddress.Create(
                        "Страна",
                        "Регион",
                        "Город",
                        "Почтовый индекс",
                        "Улица",
                        "Номер дома").Value,
                    LocationTimeZone.Create("Europe/Moscow").Value);

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Id;
            });
        }

        private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
        {
            var scope = Services.CreateAsyncScope();

            var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();

            return await action(sut);
        }
    }
}
