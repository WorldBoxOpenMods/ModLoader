using UnityEngine;

namespace NeoModLoader.General.Event.Handlers;

/// <summary>
/// This handler is made for making your own log message
/// </summary>
public abstract class WorldLogMessageHandler : AbstractHandler<WorldLogMessageHandler>
{
    /// <summary>
    /// This method is called when a log message is going to show. Detailedly, at the end of <see cref="WorldLogMessageExtensions.getFormatedText"/>
    /// </summary>
    /// <param name="pMessage">This includes message data</param>
    /// <param name="pText">The text to display</param>
    /// <param name="pColor">The color of text to show</param>
    /// <param name="pColorField">Whether <paramref name="pColor"/> is available</param>
    /// <param name="pColorTags"></param>
    public abstract void Handle(ref WorldLogMessage pMessage, ref string pText, ref Color pColor, ref bool pColorField,
        bool pColorTags);
}