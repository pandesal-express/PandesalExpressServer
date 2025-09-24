using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Services;

public interface IInventoryAdjustmentService
{
    /// <summary>
    ///     Adjusts inventory quantities for a completed transfer
    /// </summary>
    /// <param name="transferRequest">The completed transfer request</param>
    /// <param name="cancellationToken"> Cancellation token </param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task AdjustInventoryForTransferAsync(TransferRequest transferRequest, CancellationToken cancellationToken);
}
