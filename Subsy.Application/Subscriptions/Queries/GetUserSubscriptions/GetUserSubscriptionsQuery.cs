using MediatR;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetUserSubscriptions;

public sealed record GetUserSubscriptionsQuery(string UserId) : IRequest<List<SubscriptionDto>>;
