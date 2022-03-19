using Demo.Domain.Shared.Entities;
using Demo.Domain.Shared.Interfaces;
using Demo.Domain.User;
using System.Collections.Generic;

namespace Demo.Domain.Role
{
    public partial class Role : SoftDeleteEntity, IQueryableEntity
    {
        public string Name { get; set; }

        public List<UserRole> UserRoles { get; set; }
    }
}