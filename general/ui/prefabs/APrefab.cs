using UnityEngine;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
/// Abstract class for prefabs
/// </summary>
/// <typeparam name="T">Type of the actual prefab</typeparam>
public abstract class APrefab<T> : MonoBehaviour where T : APrefab<T>
{
    /// <summary>
    ///     If the prefab is initialized
    /// </summary>
    protected bool Initialized;

    /// <summary>
    /// The prefab instance
    /// </summary>
    public static T Prefab { get; protected set; }

    /// <summary>
    ///     Used to initialize the prefab. It should be called everywhere if the prefab might be not initialized.
    /// </summary>
    /// <remarks>
    ///     An instance of prefab might not call Awake() before it is used. So Init() should be called everywhere if the prefab
    ///     might be not initialized.
    /// </remarks>
    protected virtual void Init()
    {
        if (Initialized)
        {
            return;
        }

        Initialized = true;
    }
}