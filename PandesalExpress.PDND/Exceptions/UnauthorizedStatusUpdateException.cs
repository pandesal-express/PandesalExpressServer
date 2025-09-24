namespace PandesalExpress.PDND.Exceptions;

public class UnauthorizedStatusUpdateException(string status) 
    : UnauthorizedAccessException($"User not authorized to update status to {status}");