using MediatR;
using RoyalStore.Application.Abstractions.Data;
using RoyalStore.Application.Abstractions.Fiscal;
using RoyalStore.Domain.Common;
using RoyalStore.Domain.Sales;

namespace RoyalStore.Application.Fiscal.Commands.ProcessFiscalInvoice;

public sealed class ProcessFiscalInvoiceCommandHandler : IRequestHandler<ProcessFiscalInvoiceCommand, Result>
{
    private readonly ISaleRepository _saleRepository;
    private readonly IFiscalApiService _fiscalApiService;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessFiscalInvoiceCommandHandler(
        ISaleRepository saleRepository,
        IFiscalApiService fiscalApiService,
        IUnitOfWork unitOfWork)
    {
        _saleRepository = saleRepository;
        _fiscalApiService = fiscalApiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProcessFiscalInvoiceCommand request, CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetByIdAsync(request.SaleId, cancellationToken);

        if (sale is null)
            return Result.Failure(Error.NotFound("sale.not_found", $"Sale {request.SaleId} not found."));

        if (sale.FiscalStatus != InvoiceStatus.Pending)
            return Result.Success();

        var fiscalResult = await _fiscalApiService.EmitInvoiceAsync(sale, cancellationToken);

        if (fiscalResult.IsSuccess)
            sale.UpdateFiscalStatus(InvoiceStatus.Authorized, null, fiscalResult.ReceiptPrintUrl, null);
        else if (fiscalResult.IsDefinitiveFailure)
            sale.UpdateFiscalStatus(InvoiceStatus.Rejected, null, null, fiscalResult.ErrorMessage);
        else
            sale.UpdateFiscalStatus(InvoiceStatus.Pending, null, null, fiscalResult.ErrorMessage);

        await _saleRepository.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
