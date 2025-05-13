using System.Reflection;
namespace NeoModLoader.utils
{
    /// <summary>
    /// extentions for delegates, meant for serializing them
    /// </summary>
    public static class DelegateExtentions
    {
        static Type GetTypeDeepSearch(string TypeName)
        {
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in a.GetTypes())
                {
                    if(t.FullName == TypeName)
                    {
                        return t;
                    }
                }
            }
            return null;
        }
        internal static Type[] ToTypes(this ParameterInfo[] parameters)
        {
            Type[] types = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                types[i] = parameters[i].ParameterType;
            }
            return types;
        }
        /// <summary>
        /// converts a string which is a list of objects split by ',' with each object being a class+methodname, combined to a single delegate
        /// </summary>
        /// <remarks>
        /// An Example would be "Randy+randomInt,Unity.Mathematics.Random+NextInt"
        /// </remarks>
        public static Delegate ConvertToDelegate(this string String, Type DelegateType, Type[] Parameters)
        {
            string[] Delegates = String.Split(',');
            Delegate[] action = new Delegate[Delegates.Length];
            for (int i =0; i < Delegates.Length; i++)
            {
                string[] MethodInfos = Delegates[i].Split('+');
                
                var m = GetTypeDeepSearch(MethodInfos[0]).GetMethod(MethodInfos[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Parameters, null);
                action[i] = m.CreateDelegate(DelegateType);
            }
            return Delegate.Combine(action);
        }
        /// <summary>
        /// converts a delegate to a string which is a list of objects split by ',' with each object being a class.methodname
        /// </summary>
        /// <remarks>
        /// An Example would be delegate Randy.randomInt and Unity.Mathematics.Random.NextInt would become "Randy+randomInt,Unity.Mathematics.Random+NextInt"
        /// </remarks>
        public static string ConvertToString(this Delegate pDelegate)
        {
            if (pDelegate == null)
            {
                return "";
            }
            string text;
            List<string> tStringToPrint = new List<string>();
            Delegate[] invocationList = pDelegate.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                Delegate tObject = invocationList[i];
                tStringToPrint.Add($"{tObject.Method.DeclaringType.FullName}+{tObject.Method.Name}");
            }
            text = string.Join(",", tStringToPrint.ToArray());
            return text;
        }
    }
}
