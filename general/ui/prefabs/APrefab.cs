using UnityEngine;

namespace NeoModLoader.General.UI.Prefabs;

public abstract class APrefab<T> : MonoBehaviour where T : APrefab<T>
{
    public static T Prefab { get; protected set; }
}