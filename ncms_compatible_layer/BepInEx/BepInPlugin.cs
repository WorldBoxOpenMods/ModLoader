namespace BepInEx;

#pragma warning disable CS1591 // No comment for NCMS compatible layer
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class BepInPlugin : Attribute
{
    public BepInPlugin(string id, string name, string version)
    {
    }
}