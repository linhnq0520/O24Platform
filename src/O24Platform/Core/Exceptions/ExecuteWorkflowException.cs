namespace O24OpenAPI.Core.Exceptions;

/// <summary>
/// The execute workflow exception class
/// </summary>
/// <seealso cref="Exception"/>
public class ExecuteWorkflowException(string message) : Exception(message) { }
