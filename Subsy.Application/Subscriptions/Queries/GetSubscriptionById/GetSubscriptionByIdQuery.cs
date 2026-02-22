using MediatR;
using Subsy.Application.Subscriptions.Queries.Common;

namespace Subsy.Application.Subscriptions.Queries.GetSubscriptionById;

public sealed record GetSubscriptionByIdQuery(int Id, string UserId) : IRequest<SubscriptionDto?>;