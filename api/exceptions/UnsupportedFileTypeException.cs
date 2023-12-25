namespace NeoModLoader.api.exceptions;

/// <inheritdoc />
public class UnsupportedFileTypeException : IOException
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public UnsupportedFileTypeException(string filePath) : base($"Unsupported file type for path {filePath}")
    {
    }
}