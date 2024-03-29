﻿using Demo.Application.Shared.Interfaces;
using MediatR;

namespace Demo.Application.CurrentUser.Queries.GetCurrentUserFeatureFlags;

public class GetCurrentUserFeatureFlagsQuery : IQuery, IRequest<GetCurrentUserFeatureFlagsQueryResult>
{
}
