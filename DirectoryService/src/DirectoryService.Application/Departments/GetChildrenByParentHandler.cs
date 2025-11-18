using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
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
        private readonly HybridCache _cache;

        public GetChildrenByParentHandler(IDbConnectionFactory dbConnectionFactory, HybridCache cache)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _cache = cache;
        }
        public async Task<GetChildrenByParentResponse?> Handle(GetChildrenByParentCommand command, CancellationToken cancellationToken)
        {
            var size = command.Request.Size;

            var page = (command.Request.Page - 1) * size;

            var parentId = command.Request.ParentId;

            var cacheKey = $"departments_children_parent={parentId}_page={page}_size={size}";

            var children = await _cache.GetOrCreateAsync<GetChildrenByParentResponse?>(
                cacheKey,
                async ct =>
                {
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
                },
                tags: new[] { $"departmentsCache_tag" },
                cancellationToken: cancellationToken);

            return children;
        }
    }
}
