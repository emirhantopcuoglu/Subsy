using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subsy.Application.Subscriptions.Commands.ArchiveSubscription;
using Subsy.Application.Subscriptions.Commands.CreateSubscription;
using Subsy.Application.Subscriptions.Commands.DeleteSubscription;
using Subsy.Application.Subscriptions.Commands.MarkSubscriptionAsPaid;
using Subsy.Application.Subscriptions.Commands.UnarchiveSubscription;
using Subsy.Application.Subscriptions.Commands.UpdateSubscription;
using Subsy.Application.Subscriptions.Queries.GetActiveSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetDueSubscriptions;
using Subsy.Application.Subscriptions.Queries.GetSubscriptionById;
using System.Security.Claims;

namespace Subsy.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetSubscriptionByIdQuery(id, UserId), ct);

        if (result is null) return NotFound(new { message = "Subscription not found." });
        return Ok(result);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveSubscriptionsQuery(UserId), ct);
        return Ok(result);
    }

    [HttpGet("due")]
    public async Task<IActionResult> GetDue(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDueSubscriptionsQuery(UserId), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request, CancellationToken ct)
    {
        await _mediator.Send(new CreateSubscriptionCommand(
            UserId, request.Name, request.Price, request.Currency,
            request.RenewalPeriodDays, request.SelectedMonth, request.SelectedDay), ct);

        return Created();
    }

    [HttpPost("{id}/pay")]
    public async Task<IActionResult> MarkAsPaid(int id, CancellationToken ct)
    {
        await _mediator.Send(new MarkSubscriptionAsPaidCommand(id, UserId), ct);
        return Ok(new { message = "Subscription renewed." });
    }

    [HttpPost("{id}/archive")]
    public async Task<IActionResult> Archive(int id, CancellationToken ct)
    {
        await _mediator.Send(new ArchiveSubscriptionCommand(id, UserId), ct);
        return Ok(new { message = "Subscription archived." });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubscriptionRequest request, CancellationToken ct)
    {
        await _mediator.Send(new UpdateSubscriptionCommand(
            id, UserId, request.Name, request.Price, request.Currency,
            request.RenewalPeriodDays, request.SelectedMonth, request.SelectedDay), ct);

        return Ok(new { message = "Subscription updated." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteSubscriptionCommand(id, UserId), ct);
        return Ok(new { message = "Subscription deleted." });
    }

    [HttpPost("{id}/unarchive")]
    public async Task<IActionResult> Unarchive(int id, CancellationToken ct)
    {
        await _mediator.Send(new UnarchiveSubscriptionCommand(id, UserId), ct);
        return Ok(new { message = "Subscription unarchived." });
    }
}

public record CreateSubscriptionRequest(
    string Name, decimal Price, string Currency,
    int RenewalPeriodDays, int SelectedMonth, int SelectedDay);

public record UpdateSubscriptionRequest(
    string Name, decimal Price, string Currency,
    int RenewalPeriodDays, int SelectedMonth, int SelectedDay);