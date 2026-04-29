using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.SendInvoice;

public sealed class SendInvoiceCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<SendInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(SendInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.UserId == cmd.UserId, ct);

        if (invoice is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var asOfUtc = timeProvider.GetUtcNow();
        var sendResult = invoice.Send(asOfUtc);
        if (sendResult.IsError)
            return sendResult.Errors;

        await context.SaveChangesAsync(ct);

        return invoice.ToDto(asOfUtc);
    }
}
