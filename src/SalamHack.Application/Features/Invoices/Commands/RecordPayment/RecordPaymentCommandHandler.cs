using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.RecordPayment;

public sealed class RecordPaymentCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<RecordPaymentCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(RecordPaymentCommand cmd, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.UserId == cmd.UserId, ct);

        if (invoice is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var paymentResult = invoice.RecordPayment(
            cmd.Amount,
            cmd.Method,
            cmd.PaymentDate,
            cmd.Notes,
            cmd.Currency);

        if (paymentResult.IsError)
            return paymentResult.Errors;

        context.Payments.Add(paymentResult.Value);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApplicationErrors.Invoices.InvoiceNotFound;
        }

        context.ClearChangeTracker();

        var savedInvoice = await context.Invoices
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.UserId == cmd.UserId, ct);

        return savedInvoice is null
            ? ApplicationErrors.Invoices.InvoiceNotFound
            : savedInvoice.ToDto(timeProvider.GetUtcNow());
    }
}
