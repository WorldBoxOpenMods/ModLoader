using System.Runtime.Serialization;
using JetBrains.Annotations;
namespace NeoModLoader.api;

/// <summary>
/// An exception that features can intentionally throw when they fail to load.
/// </summary>
public class FeatureLoadException : Exception
{
    /// <inheritdoc/>
    protected FeatureLoadException([NotNull]SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
    /// <inheritdoc/>
    public FeatureLoadException(string message) : base(message)
    {
    }
    /// <inheritdoc/>
    public FeatureLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}