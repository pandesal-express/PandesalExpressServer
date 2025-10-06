namespace PandesalExpress.Auth.Exceptions;

public sealed class DuplicateEmailException(string message) : Exception(message);
