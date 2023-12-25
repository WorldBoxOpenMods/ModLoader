namespace NeoModLoader.api.exceptions;
/// <summary>
/// 
/// </summary>
public class UnrecognizableResourceFileException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">Path to unrecognized resource file</param>
    public UnrecognizableResourceFileException(string path) : base($"Unrecognizable resource file: {path}")
    {
    }
}