using System.Reflection;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
namespace ReflectionUtility
{
    [Obsolete("Compatible Layer will not be maintained and be removed in the future")]
    public static class Reflection
    {
        public static object CallMethod(this object o, string methodName, params object[] args)
        {
            Type type = o.GetType();
            MethodInfo method = type.GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new MissingMethodException(type.Name, methodName);
            }

            return method.Invoke(o, args);
        }

        public static object CallStaticMethod(Type type, string methodName, params object[] args)
        {
            MethodInfo method = type.GetMethod(methodName,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
            {
                throw new MissingMethodException(type.Name, methodName);
            }

            return method.Invoke(null, args);
        }

        public static object GetField(Type type, object instance, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }

            return field.GetValue(instance);
        }

        public static void SetField<T>(object originalObject, string fieldName, T newValue)
        {
            Type type = originalObject.GetType();
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }

            field.SetValue(originalObject, newValue);
        }

        public static void SetStaticField<T>(Type objectType, string fieldName, T newValue)
        {
            Type type = objectType.GetType();
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(type.Name, fieldName);
            }

            field.SetValue(null, newValue);
        }
    }
}