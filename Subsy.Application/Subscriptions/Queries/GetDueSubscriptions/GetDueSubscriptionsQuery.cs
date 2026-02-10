using MediatR;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;
public sealed record GetDueSubscriptionsQuery(string UserId) : IRequest<List<SubscriptionDto>>;
