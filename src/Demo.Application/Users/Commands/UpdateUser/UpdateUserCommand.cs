﻿using System;
using System.Collections.Generic;
using Demo.Application.Shared.Interfaces;
using Demo.Application.Users.Commands.UpdateUser.Dtos;
using Demo.Application.Users.Queries.GetUserById.Dtos;
using MediatR;

namespace Demo.Application.Users.Commands.UpdateUser;

public class UpdateUserCommand : ICommand, IRequest<Unit>
{
    internal Guid Id { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public string MiddleName { get; set; }
    public string Email { get; set; }
    public string ExternalId { get; set; }
    public GenderEnum? Gender { get; set; }
    public UserTypeEnum UserType { get; set; }
    public DateTime? BirthDate { get; set; }
    public List<UpdateUserCommandUserRoleDto> UserRoles { get; set; }

    // ReSharper disable once InconsistentNaming
    public uint xmin { get; set; }

    public void SetUserId(Guid id)
    {
        Id = id;
    }
}
