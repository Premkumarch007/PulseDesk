using MediatR;
using PulseDesk.Application.Interfaces.Services;
using PulseDesk.Shared.DTOs.Users;
using PulseDesk.Shared.Wrappers;
using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulseDesk.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, ApiResponse<PagedResponse<UserListItemDto>>>
    {
        private readonly IDbConnectionFactory _db;
        public GetUsersQueryHandler(IDbConnectionFactory bd)
        {
            _db = bd;
        }
        private static readonly HashSet<string> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
        {
            "FirstName", "LastName", "Role", "CreatedAt", "LastLoginAt","Email"
        };

        public async Task<ApiResponse<PagedResponse<UserListItemDto>>>Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var dto = request.Request;
            var offset = (dto.Page - 1) * dto.PageSize;

            // Performing Sorting Operations
            string sortBy = AllowedSortColumns.Contains(dto.SortBy) ? dto.SortBy : "CreatedAt";
            string sortOrder = dto.SortDirection.ToUpper() == "ASC" ? "ASC" : "DESC";

            var where = new StringBuilder("Where 1 = 1");

            if (!string.IsNullOrWhiteSpace(dto.Search))
                where.Append(@" AND (
                                u.FirstName LIKE @Search OR
                                u.Lastname LIKE @Search OR
                                u.Email LIKE @Search OR
                                u.Department LIKE @Search)");

            if (!string.IsNullOrWhiteSpace(dto.Role))
                where.Append(" AND r.Name = @Role");

            if (dto.IsActive.HasValue)
                where.Append(" AND u.IsActive = @IsActive");

            var sql = $@"
            SELECT
                u.Id,
                u.FirstName,
                u.LastName,
                CONCAT(u.FirstName, ' ', u.LastName) AS FullName,
                u.Email,
                r.Name        AS Role,
                u.Department,
                u.JobTitle,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt
            FROM PulseDesk.Users u
            INNER JOIN PulseDesk.Roles r ON r.Id = u.RoleId
            {where}
            ORDER BY {sortBy} {sortOrder}
            OFFSET @Offset ROWS
            FETCH NEXT @PageSize ROWS ONLY;

            -- Second query in same round trip — gets total count
            -- Without this we'd need a second DB call
            SELECT COUNT(*)
            FROM PulseDesk.Users u
            INNER JOIN PulseDesk.Roles r ON r.Id = u.RoleId
            {where};";

            var parameters = new
            {
                Search = $"%{dto.Search}%",
                Role = dto.Role,
                IsActive = dto.IsActive,
                Offset = offset,
                PageSize = dto.PageSize,
            };
            using var connection = _db.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(sql, parameters);

            var items = (await multi.ReadAsync<UserListItemDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            var response = new PagedResponse<UserListItemDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = dto.PageSize,
                Page = dto.Page
            };
            return ApiResponse<PagedResponse<UserListItemDto>>.Ok(response);
        }
    }
}
