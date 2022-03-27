using Demo.Application.Roles.Queries.SearchRoles.Dtos;
using Demo.Application.Shared.Models;
using System.Collections.Generic;

namespace Demo.Application.Roles.Queries.SearchRoles
{
    public class SearchRolesQueryResult : BasePaginatedResult
    {
        public IEnumerable<SearchRoleDto> Roles { get; set; }
    }
}