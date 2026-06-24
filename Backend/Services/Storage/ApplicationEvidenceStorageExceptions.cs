namespace Backend.Services.Storage;

public class ApplicationEvidenceStorageException : Exception
{
    public ApplicationEvidenceStorageException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

public class ApplicationEvidenceObjectNotFoundException : ApplicationEvidenceStorageException
{
    public ApplicationEvidenceObjectNotFoundException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
