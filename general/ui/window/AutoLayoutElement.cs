using NeoModLoader.General.UI.Prefabs;
using UnityEngine;

namespace NeoModLoader.General.UI.Window;

public abstract class AutoLayoutElement<T> : APrefab<T> where T : AutoLayoutElement<T>
{
}