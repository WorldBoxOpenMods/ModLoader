using UnityEngine;

namespace NeoModLoader.General.UI.Prefabs;
/// <summary>
/// Abstract class for prefabs
/// </summary>
/// <typeparam name="T">Type of the actual prefab</typeparam>
public abstract class APrefab<T> : MonoBehaviour where T : APrefab<T>
{
    /// <summary>
    /// The prefab instance
    /// </summary>
    public static T Prefab { get; protected set; }
}