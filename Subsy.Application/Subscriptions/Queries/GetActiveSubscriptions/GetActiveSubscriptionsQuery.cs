using MediatR;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetActiveSubscriptions;
public sealed record GetActiveSubscriptionsQuery(string UserId) : IRequest<List<SubscriptionDto>>;
