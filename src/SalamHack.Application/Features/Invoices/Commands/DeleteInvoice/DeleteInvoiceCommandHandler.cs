using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.DeleteInvoice;

public sealed class DeleteInvoiceCommandHandler(IAppDbContext context, TimeProvider timeProvider)
    : IRequestHandler<DeleteInvoiceCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteInvoiceCommand cmd, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.UserId == cmd.UserId, ct);

        if (invoice is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        if (invoice.Status != InvoiceStatus.Draft)
            return InvoiceErrors.OnlyDraftCanBeDeleted;

        invoice.Delete(timeProvider.GetUtcNow());
        await context.SaveChangesAsync(ct);

        return Result.Deleted;
    }
}
