using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace NeoModLoader.General.Event.Handlers;
/// <summary>
/// This handler is made for making your own log message
/// </summary>
public abstract class WorldLogMessageHandler : AbstractHandler<WorldLogMessageHandler>
{
    /// <summary>
    /// This method is called when a log message is going to show. Detailedly, at the end of <see cref="WorldLogMessageExtensions.getFormatedText"/>
    /// </summary>
    /// <param name="pMessage"></param>
    /// <param name="pText"></param>
    /// <param name="pColor"></param>
    /// <param name="pColorField"></param>
    /// <param name="pColorTags"></param>
    public abstract void Handle(ref WorldLogMessage pMessage, ref string pText, ref Color pColor, ref bool pColorField, bool pColorTags);
}
