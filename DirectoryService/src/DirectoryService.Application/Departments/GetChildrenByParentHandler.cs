using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record GetChildrenByParentCommand(GetChildrenByParentRequest Request);
    public class GetChildrenByParentHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public GetChildrenByParentHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        public async Task<GetChildrenByParentResponse?> Handle(GetChildrenByParentCommand command, CancellationToken cancellationToken)
        {
            var size = command.Request.Size;

            var page = (command.Request.Page - 1) * size;

            var parentId = command.Request.ParentId;

            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var children = await connection.QueryAsync<DepartmentWithHasMoreChildrenDto>(
                $"""
                WITH children AS
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
                WHERE d.parent_id = @parentId
                LIMIT {size} OFFSET {page})
                
                SELECT *, (EXISTS (SELECT 1 FROM departments WHERE parent_id = children.id OFFSET {page} LIMIT 1))
                AS hasMoreChildren
                FROM children
                """,
                new { parentId });

            return new GetChildrenByParentResponse
            {
                DepartmentsWithHasMoreChildren = children.ToList()
            };
        }
    }
}
