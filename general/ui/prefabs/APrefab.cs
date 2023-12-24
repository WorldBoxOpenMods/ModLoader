using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
/// Abstract class for prefabs. If you implement 'void _init(void)', it will be called when the prefab is used for the first time. Otherwise, you need to initialize the prefab manually.
/// </summary>
/// <typeparam name="T">Type of the actual prefab</typeparam>
public abstract class APrefab<T> : MonoBehaviour where T : APrefab<T>
{
    /// <summary>
    ///     If the prefab is initialized
    /// </summary>
    protected bool Initialized;
    private static T mPrefab;
    /// <summary>
    /// The prefab instance
    /// </summary>
    public static T Prefab {
        get
        {
            if(mPrefab == null)
            {
                if (OtherUtils.CalledBy("_init", typeof(T), true))
                {
                    return null;
                }
                typeof(T).GetMethod("_init", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).Invoke(null, null);
            }
            return mPrefab;
        } 
        protected set {
            mPrefab = value;
        }
    }

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