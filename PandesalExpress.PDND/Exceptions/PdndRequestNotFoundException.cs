namespace PandesalExpress.PDND.Exceptions;

public class PdndRequestNotFoundException(string requestId) 
    : Exception($"PDND request {requestId} not found");