using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NCMS.Utils
{
    public class GameObjects
    {
        public static GameObject FindEvenInactive(string Name)
        {
            GameObject[] array = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in array)
            {
                if (obj.name == Name)
                {
                    return obj;
                }
            }
            return null;
        }
    }
}
