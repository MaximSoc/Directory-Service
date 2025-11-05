using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using FluentValidation;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record GetParentWithChildrensCommand(GetParentWithChildrensRequest Request);

    public class GetParentWithChildrensHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public GetParentWithChildrensHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        public async Task<GetParentWithChildrensResponse?> Handle(GetParentWithChildrensCommand command, CancellationToken cancellationToken)
        {
            var size = command.Request.Size;

            var page = (command.Request.Page - 1) * size;

            var prefetch = command.Request.Prefetch;

            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var departmentsWithHasMoreChildren = await connection.QueryAsync<DepartmentWithHasMoreChildrenDto>(
                $"""
                WITH roots AS
                (SELECT d.id,
                d.name,
                d.identifier,
                d.parent_id AS parentId,
                d.path,
                d.depth,
                d.is_active AS isActive,
                d.created_at AS createdAt,
                d.updated_at AS updatedAt
                FROM departments d
                WHERE d.parent_id IS NULL
                LIMIT {size} OFFSET {page})

                SELECT *, (EXISTS (SELECT 1 FROM departments WHERE parent_id = roots.id OFFSET {prefetch} LIMIT 1))
                AS hasMoreChildren
                FROM roots

                UNION ALL

                SELECT c.*, (EXISTS (SELECT 1 FROM departments WHERE parent_id = c.id))
                AS hasMoreChildren FROM roots r
                CROSS JOIN LATERAL (SELECT d.id,
                d.name,
                d.identifier,
                d.parent_id AS parentId,
                d.path,
                d.depth,
                d.is_active AS isActive,
                d.created_at AS createdAt,
                d.updated_at AS updatedAt
                FROM departments d
                WHERE parent_id = r.id
                LIMIT {prefetch}) c
                """);

            return new GetParentWithChildrensResponse
            {
                DepartmentsWithHasMoreChildren = departmentsWithHasMoreChildren.ToList()
            };
        }
    }
}
