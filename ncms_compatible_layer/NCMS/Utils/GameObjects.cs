using UnityEngine;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace NCMS.Utils
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public class GameObjects
    {
        /// <remarks>
        ///     From [NCMS](https://denq04.github.io/ncms/)
        /// </remarks>
        [Obsolete("Use ResourcesFinder.FindResources<T>(string name) instead")]
        public static GameObject FindEvenInactive(string Name)
        {
            var array = Resources.FindObjectsOfTypeAll<GameObject>();
            return array.FirstOrDefault(obj =>
                string.Equals(obj.name, Name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}