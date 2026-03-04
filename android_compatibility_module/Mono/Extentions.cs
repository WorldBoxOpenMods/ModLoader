using UnityEngine;

namespace NeoModLoader.AndroidCompatibilityModule;

public static class Extentions
{
    public static T GetWrappedComponent<T>(this GameObject obj)
    {
        return obj.GetComponent<T>();
    }
}