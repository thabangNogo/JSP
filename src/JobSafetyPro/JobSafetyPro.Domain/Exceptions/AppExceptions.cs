namespace JobSafetyPro.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Entity \"{name}\" ({key}) was not found.")
    {
    }
}

public class UnauthorizedAppException : Exception
{
    public UnauthorizedAppException(string message) : base(message)
    {
    }
}

public class ForbiddenAppException : Exception
{
    public ForbiddenAppException(string message) : base(message)
    {
    }
}

public class ValidationAppException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IDictionary<string, string[]> errors)
        : base("One or more validation failures occurred.")
    {
        Errors = errors;
    }
}
