namespace OutletControl.Application.Interfaces.Receiving;

public interface IReceivingExportService
{
    Task<byte[]> ExportReceiptAsync(
        int outletId,
        int receiptId);
}