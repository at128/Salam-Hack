using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.UpdatePayment;

public sealed class UpdatePaymentCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<UpdatePaymentCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(UpdatePaymentCommand cmd, CancellationToken ct)
    {
        var payment = await context.Payments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == cmd.PaymentId && p.Invoice.UserId == cmd.UserId, ct);

        if (payment is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var invoice = payment.Invoice;
        var updateResult = invoice.UpdatePayment(
            payment,
            cmd.Amount,
            cmd.Method,
            cmd.PaymentDate,
            cmd.Notes,
            cmd.Currency,
            timeProvider.GetUtcNow());

        if (updateResult.IsError)
            return updateResult.Errors;

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
            .FirstOrDefaultAsync(i => i.Id == invoice.Id && i.UserId == cmd.UserId, ct);

        return savedInvoice is null
            ? ApplicationErrors.Invoices.InvoiceNotFound
            : savedInvoice.ToDto(timeProvider.GetUtcNow());
    }
}
