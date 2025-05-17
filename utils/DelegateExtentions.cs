using System.Reflection;
namespace NeoModLoader.utils
{
    /// <summary>
    /// extentions for delegates, meant for serializing them
    /// </summary>
    public static class DelegateExtentions
    {
        /// <summary>
        /// converts a string which is a list of objects split by '+' with each object being a class:methodname, combined to a single delegate
        /// </summary>
        /// <remarks>
        /// An Example would be "Randy:randomInt+Unity.Mathematics.Random:NextInt"
        /// </remarks>
        public static Delegate ConvertToDelegate(this string String, Type DelegateType)
        {
            string[] Delegates = String.Split('+');
            Delegate[] delegates = new Delegate[Delegates.Length];
            for (int i =0; i < Delegates.Length; i++)
            {
                string[] MethodInfos = Delegates[i].Split(':');
                var m = Type.GetType(MethodInfos[0]).GetMethod(MethodInfos[1], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                delegates[i] = m.CreateDelegate(DelegateType);
            }
            return Delegate.Combine(delegates);
        }
        /// <summary>
        /// converts a delegate to a string which is a list of objects split by '+' with each object being a class:methodname
        /// </summary>
        /// <remarks>
        /// An Example would be delegate Randy.randomInt and Unity.Mathematics.Random.NextInt would become "Randy:randomInt+Unity.Mathematics.Random:NextInt"
        /// </remarks>
        public static string ConvertToString(this Delegate pDelegate)
        {
            if (pDelegate == null)
            {
                return "";
            }
            string text;
            List<string> tStringToPrint = new();
            Delegate[] invocationList = pDelegate.GetInvocationList();
            for (int i = 0; i < invocationList.Length; i++)
            {
                Delegate tObject = invocationList[i];
                tStringToPrint.Add($"{tObject.Method.DeclaringType.AssemblyQualifiedName}:{tObject.Method.Name}");
            }
            text = string.Join("+", tStringToPrint.ToArray());
            return text;
        }
    }
}
