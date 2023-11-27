namespace BepInEx;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BepInPlugin : Attribute
{
    public BepInPlugin(string id, string name, string version)
    {
    }
}