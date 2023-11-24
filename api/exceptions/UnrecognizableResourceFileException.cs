namespace NeoModLoader.api.exceptions;

public class UnrecognizableResourceFileException : Exception
{
    public UnrecognizableResourceFileException(string path) : base($"Unrecognizable resource file: {path}")
    {
    }
}