﻿using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Application.Users.Queries.GetUserById.Dtos;
using Demo.Domain.Shared.Interfaces;
using Demo.Domain.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Application.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, GetUserByIdQueryResult>
{
    private readonly IMapper _mapper;
    private readonly IDbQuery<User> _query;

    public GetUserByIdQueryHandler(
        IDbQuery<User> query,
        IMapper mapper
    )
    {
        _query = query;
        _mapper = mapper;
    }

    public async Task<GetUserByIdQueryResult> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _query.AsQueryable()
            .Include(user => user.UserRoles)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        return new GetUserByIdQueryResult { User = user };
    }
}
