using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.RecordAdvancePayment;

public sealed class RecordAdvancePaymentCommandHandler(
    IAppDbContext context,
    TimeProvider timeProvider)
    : IRequestHandler<RecordAdvancePaymentCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(RecordAdvancePaymentCommand cmd, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.UserId == cmd.UserId, ct);

        if (invoice is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var paymentResult = invoice.RecordAdvancePayment(
            cmd.Method,
            cmd.PaymentDate,
            cmd.Currency,
            cmd.Notes);

        if (paymentResult.IsError)
            return paymentResult.Errors;

        await context.SaveChangesAsync(ct);

        return invoice.ToDto(timeProvider.GetUtcNow());
    }
}
