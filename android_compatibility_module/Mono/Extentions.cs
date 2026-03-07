using UnityEngine;

namespace NeoModLoader.AndroidCompatibilityModule;

public static class Extentions
{
    public static T GetWrappedComponent<T>(this GameObject obj)
    {
        return obj.GetComponent<T>();
    }
    public static T GetPointer<T>(this T obj)
        {
            return obj;
        }

    public static List<Transform> GetChildren(this Transform transform)
    {
        List<Transform> list = new List<Transform>();
        foreach (Transform tr in transform)
        {
            list.Add(tr);
        }

        return list;
    }
}