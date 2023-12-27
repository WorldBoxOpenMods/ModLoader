using NeoModLoader.General.UI.Prefabs;

namespace NeoModLoader.General.UI.Window;

/// <summary>
///     Abstract class of auto layout element
/// </summary>
public abstract class AutoLayoutElement<T> : APrefab<T> where T : AutoLayoutElement<T>
{
}