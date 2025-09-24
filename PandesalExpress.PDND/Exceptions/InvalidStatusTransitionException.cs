namespace PandesalExpress.PDND.Exceptions;

public class InvalidStatusTransitionException(string currentStatus, string newStatus) 
    : Exception($"Cannot transition from {currentStatus} to {newStatus}");