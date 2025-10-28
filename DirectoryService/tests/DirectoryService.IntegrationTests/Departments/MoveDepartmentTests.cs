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
    public class MoveDepartmentTests : DirectoryTestsBase
    {
        public MoveDepartmentTests(DirectoryTestWebFactory factory)
            : base(factory) { }

        [Fact]
        public async Task MoveChildren_to_without_parent_should_succeed()
        {
            //arrange
            Guid locationId = await CreateLocation("Location");

            Guid parentId = await CreateParent("Parent", locationId);
            var parent = await ExecuteInDb(async dbContext =>
            {
                return await dbContext.Departments.FirstAsync(d => d.Id == parentId);
            });

            Guid childrenId = await CreateChildren("Children", locationId, parent);

            var cancellationToken = CancellationToken.None;

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new MoveDepartmentCommand(new Contracts.Departments.MoveDepartmentRequest(
                childrenId,
                null));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(d => d.Id == childrenId, cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id, childrenId);
                Assert.True(department.Depth == 0);
                Assert.True(department.Path.Value == department.Identifier.Value);
                Assert.True(result.IsSuccess);
            });
        }

        [Fact]
        public async Task MoveChildren_to_another_parent_should_succeed()
        {
            //arrange
            Guid locationId = await CreateLocation("Location");

            Guid parentOneId = await CreateParent("ParentOne", locationId);
            var parentOne = await ExecuteInDb(async dbContext =>
            {
                return await dbContext.Departments.FirstAsync(d => d.Id == parentOneId);
            });

            Guid parentTwoId = await CreateParent("ParentTwo", locationId);

            Guid childrenId = await CreateChildren("Children", locationId, parentOne);

            var cancellationToken = CancellationToken.None;

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new MoveDepartmentCommand(new Contracts.Departments.MoveDepartmentRequest(
                childrenId,
                parentTwoId));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            await ExecuteInDb(async dbContext =>
            {
                var department = await dbContext.Departments.FirstAsync(d => d.Id == childrenId, cancellationToken);

                Assert.NotNull(department);
                Assert.Equal(department.Id, childrenId);
                Assert.True(department.Depth == 1);
                Assert.True(department.Path.Value != department.Identifier.Value);
                Assert.True(result.IsSuccess);
            });
        }

        [Fact]
        public async Task MoveChildrenWithGrandson_to_without_parent_should_succeed()
        {
            //arrange
            Guid locationId = await CreateLocation("Location");

            Guid parentOneId = await CreateParent("ParentOne", locationId);
            var parentOne = await ExecuteInDb(async dbContext =>
            {
                return await dbContext.Departments.FirstAsync(d => d.Id == parentOneId);
            });

            Guid childrenId = await CreateChildren("Children", locationId, parentOne);
            var children = await ExecuteInDb(async dbContext =>
            {
                return await dbContext.Departments.FirstAsync(d => d.Id == childrenId);
            });

            Guid grandsonId = await CreateChildren("Grandson", locationId, children);

            var cancellationToken = CancellationToken.None;

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new MoveDepartmentCommand(new Contracts.Departments.MoveDepartmentRequest(
                childrenId,
                null));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            await ExecuteInDb(async dbContext =>
            {
                var childrenAfterAction = await dbContext.Departments.FirstAsync(d => d.Id == childrenId, cancellationToken);
                var grandsonAfterAction = await dbContext.Departments.FirstAsync(d => d.Id == grandsonId, cancellationToken);

                Assert.NotNull(childrenAfterAction);
                Assert.Equal(childrenAfterAction.Id, childrenId);
                Assert.True(childrenAfterAction.Depth == 0);
                Assert.True(childrenAfterAction.Path.Value == childrenAfterAction.Identifier.Value);
                Assert.NotNull(grandsonAfterAction);
                Assert.Equal(grandsonAfterAction.Id, grandsonId);
                Assert.True(grandsonAfterAction.Depth == 1);
                Assert.True(grandsonAfterAction.Path.Value != "parent.chilgren.grandson");
                Assert.True(result.IsSuccess);
            });
        }

        [Fact]
        public async Task MoveParent_to_children_should_failed()
        {
            //arrange
            Guid locationId = await CreateLocation("Location");

            Guid parentId = await CreateParent("Parent", locationId);
            var parent = await ExecuteInDb(async dbContext =>
            {
                return await dbContext.Departments.FirstAsync(d => d.Id == parentId);
            });

            Guid childrenId = await CreateChildren("Children", locationId, parent);

            var cancellationToken = CancellationToken.None;

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new MoveDepartmentCommand(new Contracts.Departments.MoveDepartmentRequest(
                parentId,
                childrenId));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            Assert.True(result.IsFailure);
        }

        [Fact]
        public async Task MoveParent_to_yourself_should_failed()
        {
            //arrange
            Guid locationId = await CreateLocation("Location");

            Guid parentId = await CreateParent("Parent", locationId);

            var cancellationToken = CancellationToken.None;

            //act
            var result = await ExecuteHandler((sut) =>
            {
                var command = new MoveDepartmentCommand(new Contracts.Departments.MoveDepartmentRequest(
                parentId,
                parentId));

                return sut.Handle(command, cancellationToken);
            });

            //assert
            Assert.True(result.IsFailure);
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

        private async Task<Guid> CreateChildren(string name, Guid locationId, Department parent)
        {
            Guid childrenId = Guid.NewGuid();
            List<DepartmentLocation> departmentLocations = new List<DepartmentLocation>();
            departmentLocations.Add(new DepartmentLocation(childrenId, locationId));

            return await ExecuteInDb(async dbContext =>
            {
                var children = Department.CreateChild(
                    childrenId,
                    DepartmentName.Create(name).Value,
                    DepartmentIdentifier.Create("children").Value,
                    parent,
                    departmentLocations).Value;

                dbContext.Departments.Add(children);
                await dbContext.SaveChangesAsync();

                return children.Id;
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

        private async Task<T> ExecuteHandler<T>(Func<MoveDepartmentHandler, Task<T>> action)
        {
            var scope = Services.CreateAsyncScope();

            var sut = scope.ServiceProvider.GetRequiredService<MoveDepartmentHandler>();

            return await action(sut);
        }
    }
}
