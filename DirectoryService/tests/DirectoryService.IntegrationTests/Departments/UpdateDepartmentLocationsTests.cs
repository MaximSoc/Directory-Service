using DirectoryService.Application.Departments;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using DirectoryService.Domain.ValueObjects.LocationVO;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.IntegrationTests.Departments
{
    public class UpdateDepartmentLocationsTests : DirectoryTestsBase
    {
        public UpdateDepartmentLocationsTests(DirectoryTestWebFactory factory)
            : base(factory) { }

        [Fact]
        public async Task UpdateDepartmentLocation_with_valid_data_should_succeed()
        {
            //arrange
            Guid locationOneId = await CreateLocation("LocationOne", "15");
            Guid locationTwoId = await CreateLocation("LocationTwo", "20");

            var cancellationToken = CancellationToken.None;

            Guid departmentId = await CreateDepartment("Department", locationOneId);

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new UpdateDepartmentLocationsCommand(new Contracts.Departments.UpdateDepartmentLocationsRequest(
                departmentId,
                [locationTwoId]));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments
                .Include(d => d.DepartmentLocations)
                .FirstAsync(d => d.Id == departmentId, cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id, departmentId);

                Assert.True(result.IsSuccess);
                Assert.Contains<Guid>(locationTwoId, department.DepartmentLocations.Select(dl => dl.LocationId));
            });
        }

        [Fact]
        public async Task UpdateDepartmentLocation_without_locations_should_failed()
        {
            //arrange
            Guid locationOneId = await CreateLocation("LocationOne", "15");

            var cancellationToken = CancellationToken.None;

            Guid departmentId = await CreateDepartment("Department", locationOneId);

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new UpdateDepartmentLocationsCommand(new Contracts.Departments.UpdateDepartmentLocationsRequest(
                departmentId,
                []));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task UpdateDepartmentLocation_with_nonexistent_locations_should_failed()
        {
            //arrange
            Guid locationOneId = await CreateLocation("LocationOne", "15");

            var cancellationToken = CancellationToken.None;

            Guid departmentId = await CreateDepartment("Department", locationOneId);

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new UpdateDepartmentLocationsCommand(new Contracts.Departments.UpdateDepartmentLocationsRequest(
                departmentId,
                [Guid.NewGuid()]));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task UpdateDepartmentLocation_with_double_locations_should_failed()
        {
            //arrange
            Guid locationOneId = await CreateLocation("LocationOne", "15");
            Guid locationTwoId = await CreateLocation("LocationTwo", "20");

            var cancellationToken = CancellationToken.None;

            Guid departmentId = await CreateDepartment("Department", locationOneId);

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new UpdateDepartmentLocationsCommand(new Contracts.Departments.UpdateDepartmentLocationsRequest(
                departmentId,
                [locationTwoId, locationTwoId]));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            Assert.True(result.IsFailure);
        }

        private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentLocationsHandler, Task<T>> action)
        {
            var scope = Services.CreateAsyncScope();

            var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationsHandler>();

            return await action(sut);
        }

        private async Task<Guid> CreateLocation(string name, string houseNumber)
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
                        houseNumber).Value,
                    LocationTimeZone.Create("Europe/Moscow").Value);

                dbContext.Locations.Add(location);
                await dbContext.SaveChangesAsync();

                return location.Id;
            });
        }

        private async Task<Guid> CreateDepartment(string name, Guid locationId)
        {
            Guid departmentId = Guid.NewGuid();
            List<DepartmentLocation> departmentLocations = new List<DepartmentLocation>();
            departmentLocations.Add(new DepartmentLocation(departmentId, locationId));

            return await ExecuteInDb(async dbContext =>
            {
                var department = Department.CreateParent(
                    DepartmentName.Create(name).Value,
                    DepartmentIdentifier.Create("department").Value,
                    departmentLocations,
                    departmentId).Value;

                dbContext.Departments.Add(department);
                await dbContext.SaveChangesAsync();

                return department.Id;
            });
        }
    }
}
