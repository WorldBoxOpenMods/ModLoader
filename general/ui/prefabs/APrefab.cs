using System.Reflection;
using NeoModLoader.utils;
using UnityEngine;

namespace NeoModLoader.General.UI.Prefabs;

/// <summary>
/// Abstract class for prefabs. If you implement 'void _init(void)', it will be called when the prefab is used for the first time. Otherwise, you need to initialize the prefab manually.
/// </summary>
/// <remarks>
/// To standard the prefab. You would be better to initialize prefab in '_init' and call 'Setup' for setup an object <see cref="UnityEngine.Object.Instantiate{T}(T)"/> from prefab.
/// </remarks>
/// <typeparam name="T">Type of the actual prefab</typeparam>
public abstract class APrefab<T> : MonoBehaviour where T : APrefab<T>
{
    private static T mPrefab;

    /// <summary>
    ///     If the prefab is initialized
    /// </summary>
    protected bool Initialized;

    /// <summary>
    /// The prefab instance
    /// </summary>
    public static T Prefab
    {
        get
        {
            if (mPrefab == null)
            {
                if (OtherUtils.CalledBy("_init", typeof(T), true))
                {
                    return null;
                }

                typeof(T).GetMethod("_init", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.Invoke(null, null);
            }

            return mPrefab;
        }
        protected set { mPrefab = value; }
    }

    /// <summary>
    ///     Equal to <see cref="UnityEngine.Object.Instantiate{T}(T,Transform,bool)" /> and set the name of the instance.
    /// </summary>
    /// <param name="pParent"></param>
    /// <param name="pWorldPositionStays"></param>
    /// <param name="pName"></param>
    /// <returns></returns>
    public static T Instantiate(Transform pParent = null, bool pWorldPositionStays = false, string pName = null)
    {
        T t = Instantiate(Prefab, pParent, pWorldPositionStays);
        if (!string.IsNullOrEmpty(pName))
            t.name = pName;
        return t;
    }

    /// <summary>
    ///     Set size of the instance
    /// </summary>
    /// <param name="pSize">The size of the root game object</param>
    public virtual void SetSize(Vector2 pSize)
    {
        RectTransform rect = GetComponent<RectTransform>();
        if (rect == null) return;
        rect.sizeDelta = pSize;
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