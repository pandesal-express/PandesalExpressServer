using PandesalExpress.Infrastructure.Models;

namespace PandesalExpress.Transfers.Exceptions;

public class TransferStatusOutOfRangeException(TransferStatus status)
    : ArgumentOutOfRangeException($"Status {status} is not within the valid range");
