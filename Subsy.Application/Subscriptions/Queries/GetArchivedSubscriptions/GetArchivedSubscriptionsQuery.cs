using MediatR;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetArchivedSubscriptions;
public sealed record GetArchivedSubscriptionsQuery(string UserId) : IRequest<List<SubscriptionDto>>;
