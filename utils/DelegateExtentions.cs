using System.Reflection;
namespace NeoModLoader.utils
{
    /// <summary>
    /// extentions for delegates, meant for serializing them
    /// </summary>
    public static class DelegateExtentions
    {
        /// <summary>
        /// gets all of the parameters from a delegate type
        /// </summary>
        public static Type[] GetDelegateParameters(this Type delegateType)
        {
            MethodInfo method = delegateType.GetMethod("Invoke");
            ParameterInfo[] info = method.GetParameters();
            Type[] types = new Type[info.Length];
            for (int i = 0; i < info.Length; i++) {
                types[i] = info[i].ParameterType;
            }
            return types;
        }
        /// <summary>
        /// converts a string to a single delegate
        /// </summary>
        /// <remarks>
        /// An Example would be "Randy:randomInt+Unity.Mathematics.Random:NextInt"
        /// </remarks>
        /// <param name="String">a list of objects split by '+' with each object being a class:methodname</param>
        /// <param name="DelegateType">if null, the AsString function must have includetype set to true</param>
        /// <exception cref="ArgumentNullException"></exception>
        public static Delegate AsDelegate(this string String, Type DelegateType = null)
        {
            if(DelegateType == null)
            {
                string[] TypeAndDelegate = String.Split('&');
                DelegateType = Type.GetType(TypeAndDelegate[0]);
                String = TypeAndDelegate[1];
            }
            string[] DelegateIDS = String.Split('+');
            Delegate[] Delegates = new Delegate[DelegateIDS.Length];
            Type[] Parameters = DelegateType.GetDelegateParameters();
            for (int i =0; i < DelegateIDS.Length; i++)
            {
                string[] MethodPath = DelegateIDS[i].Split(':');
                var m = Type.GetType(MethodPath[0]).GetMethod(MethodPath[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Parameters, null);
                Delegates[i] = m.CreateDelegate(DelegateType);
            }
            return Delegate.Combine(Delegates);
        }
        /// <summary>
        /// converts a delegate to a string which is a list of objects split by '+' with each object being a class:methodname
        /// </summary>
        /// <remarks>
        /// An Example would be delegate Randy.randomInt and Unity.Mathematics.Random.NextInt would become "Randy:randomInt+Unity.Mathematics.Random:NextInt"
        /// </remarks>
        /// <exception cref="ArgumentNullException"></exception>
        public static string AsString(this Delegate pDelegate, bool IncludeType = false)
        {
            string text;
            List<string> tStringToPrint = new();
            Delegate[] invocationList = pDelegate.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                Delegate tObject = invocationList[i];
                tStringToPrint.Add($"{tObject.Method.DeclaringType.AssemblyQualifiedName}:{tObject.Method.Name}");
            }
            text = string.Join("+", tStringToPrint.ToArray());
            if (IncludeType)
            {
                text = string.Join("&", pDelegate.GetType().AssemblyQualifiedName, text);
            }
            return text;
        }
    }
}
