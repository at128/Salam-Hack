using SalamHack.Application.Common.Errors;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Application.Features.Invoices.Models;
using SalamHack.Domain.Common.Results;
using SalamHack.Domain.Invoices;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace SalamHack.Application.Features.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateInvoiceCommand, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(CreateInvoiceCommand cmd, CancellationToken ct)
    {
        var project = await context.Projects
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == cmd.ProjectId && p.UserId == cmd.UserId, ct);

        if (project is null)
            return ApplicationErrors.Projects.ProjectNotFound;

        var invoiceNumber = cmd.InvoiceNumber.Trim();
        var numberExists = await context.Invoices
            .AnyAsync(i => i.UserId == cmd.UserId && i.InvoiceNumber == invoiceNumber, ct);

        if (numberExists)
            return ApplicationErrors.Invoices.InvoiceNumberAlreadyExists;

        var invoiceResult = Invoice.Create(
            cmd.UserId,
            cmd.ProjectId,
            project.CustomerId,
            invoiceNumber,
            cmd.TotalAmount,
            cmd.AdvanceAmount,
            cmd.IssueDate,
            cmd.DueDate,
            cmd.Currency,
            cmd.Notes);

        if (invoiceResult.IsError)
            return invoiceResult.Errors;

        var invoice = invoiceResult.Value;
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync(ct);

        var invoiceDto = await LoadInvoiceDtoAsync(invoice.Id, cmd.UserId, ct);

        return invoiceDto is null
            ? ApplicationErrors.Invoices.InvoiceNotFound
            : invoiceDto;
    }

    private async Task<InvoiceDto?> LoadInvoiceDtoAsync(Guid invoiceId, Guid userId, CancellationToken ct)
    {
        var invoice = await context.Invoices
            .AsNoTracking()
            .Include(i => i.Project)
                .ThenInclude(p => p.Customer)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId, ct);

        return invoice?.ToDto();
    }
}
