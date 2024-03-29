﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Application.Shared.Extensions;
using Demo.Application.Shared.Models;
using Demo.Application.Users.Queries.UserLookup.Dtos;
using Demo.Domain.Shared.Interfaces;
using Demo.Domain.User;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Demo.Application.Users.Queries.UserLookup;

public class UserLookupQueryHandler : IRequestHandler<UserLookupQuery, UserLookupQueryResult>
{
    private readonly IMapper _mapper;
    private readonly IDbQuery<User> _query;

    public UserLookupQueryHandler(
        IDbQuery<User> query,
        IMapper mapper
    )
    {
        _query = query;
        _mapper = mapper;
    }

    public async Task<UserLookupQueryResult> Handle(UserLookupQuery request, CancellationToken cancellationToken)
    {
        var query = request.Ids is { Length: > 0 }
            ? _query.WithOptions(x => x.IncludeDeleted = true).AsQueryable()
            : _query.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x => EF.Functions.ILike(x.Fullname, $"%{request.SearchTerm}%"));
        }

        if (request.Ids is { Length: > 0 })
        {
            query = query.Where(x => request.Ids.Contains(x.Id));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var sortOrder = request.OrderByDescending ? SortDirection.Descending : SortDirection.Ascending;

        query = request.OrderBy switch
        {
            UserLookupOrderByEnum.Fullname => query.OrderBy(x => x.Fullname, sortOrder),
            UserLookupOrderByEnum.FamilyName => query.OrderBy(x => x.FamilyName, sortOrder),
            _ => throw new Exception($"OrderBy '{request.OrderBy}' not implemented.")
        };

        var users = await query
            .Skip(request.PageSize * request.PageIndex)
            .Take(request.PageSize)
            .ProjectTo<UserLookupDto>(_mapper.ConfigurationProvider)
            //.WriteQueryStringToOutputWindowIfInDebugMode()
            .ToListAsync(cancellationToken);

        return new UserLookupQueryResult
        {
            PageIndex = request.PageIndex, PageSize = request.PageSize, TotalItems = totalItems, Users = users
        };
    }
}
