using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.UpdateInvoiceDetails;

public sealed class UpdateInvoiceDetailsCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateInvoiceDetailsCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(UpdateInvoiceDetailsCommand cmd, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == cmd.InvoiceId && i.Project.UserId == cmd.UserId, ct);

        if (invoice is null)
            return ApplicationErrors.Invoices.InvoiceNotFound;

        var updateResult = invoice.UpdateDetails(
            cmd.TotalAmount,
            cmd.AdvanceAmount,
            cmd.IssueDate,
            cmd.DueDate,
            cmd.Currency,
            cmd.Notes);

        if (updateResult.IsError)
            return updateResult.Errors;

        await context.SaveChangesAsync(ct);

        return invoice.ToDto();
    }
}
