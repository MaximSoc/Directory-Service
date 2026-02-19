using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public class GetDepartmentByIdHandler : IQueryHandler<GetDepartmentByIdResponse, GetDepartmentByIdRequest>
    {
        private readonly IReadDbContext _dbContext;
        private readonly ILogger<GetDepartmentByIdHandler> _logger;

        public GetDepartmentByIdHandler(IReadDbContext dbContext, ILogger<GetDepartmentByIdHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<GetDepartmentByIdResponse, Errors>> Handle(GetDepartmentByIdRequest request, CancellationToken cancellationToken)
        {
            Guid departmentId = request.DepartmentId;

            var connection = _dbContext.Connection;

            var sql = """
                SELECT
                    d.id,
                    d.name,
                    d.identifier,
                    d.parent_id AS parentId,
                    d.path,
                    d.depth,
                    d.is_active AS isActive,
                    d.created_at AS createdAt,
                    d.updated_at AS updatedAt,
                    p.id,
                    p.name,
                    l.id,
                    l.name
                    FROM departments d
                    LEFT JOIN department_positions dp ON dp.department_id = d.id AND dp.is_active = true
                    LEFT JOIN positions p ON p.id = dp.position_id AND p.is_active = true
                    LEFT JOIN department_locations dl ON dl.department_id = d.id AND dl.is_active = true
                    LEFT JOIN locations l ON l.id = dl.location_id AND l.is_active = true
                    WHERE d.id = @Id
                """;

            DepartmentDto? department = null;

            await connection.QueryAsync<
                DepartmentDto,
                DepartmentPositionDto,
                DepartmentLocationDto,
                DepartmentDto>(
                sql,
                (dept, pos, loc) =>
                {
                    if (department == null)
                        department = dept;
                    if (pos != null && !department.Positions.Any(x => x.Id == pos.Id))
                        department.Positions.Add(pos);
                    if (loc != null && !department.Locations.Any(x => x.Id == loc.Id))
                        department.Locations.Add(loc);
                    return department;
                },
                new { Id = departmentId },
                splitOn: "id, id");

            if (department == null)
            {
                return GeneralErrors.NotFound(departmentId).ToErrors();
            }

            return new GetDepartmentByIdResponse
            {
                Department = department
            };
        }
    }
}
