using UnityEngine;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class GameObjects
    {
        [Obsolete("Use ResourcesFinder.FindResources<T>(string name) instead")]
        public static GameObject FindEvenInactive(string Name)
        {
            GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in array)
            {
                if (obj.name.ToLower() == Name.ToLower())
                {
                    return obj;
                }
            }

            return null;
        }
    }
}