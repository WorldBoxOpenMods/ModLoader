namespace NeoModLoader.api.exceptions;

/// <inheritdoc />
public class UnsupportedFileTypeException : IOException
{
    public UnsupportedFileTypeException(string filePath) : base($"Unsupported file type for path {filePath}")
    {
    }
}