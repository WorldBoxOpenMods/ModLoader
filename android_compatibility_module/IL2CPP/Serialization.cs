
using UnityEngine;
namespace NeoModLoader.AndroidCompatibilityModule.Serialization;
[Serializable]
public class Vector
{
    public float x;
    public float y;
    public float z;
    public Vector(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    public static implicit operator Vector3(Vector v)
    {
        return new  UnityEngine.Vector3(v.x, v.y, v.z);
    }
    public static implicit operator Vector(Vector3 v)
    {
        return new Vector(v.x, v.y, v.z);
    }
    public static implicit operator Quaternion(Vector v)
    {
        return Quaternion.Euler(v);
    }
    public static implicit operator Vector(Quaternion v)
    {
        return v.eulerAngles;
    }
}
/// <summary>
/// data for behaviours
/// </summary>
[Serializable]
public class GameObjectData
{
    public Vector Position;
    public Vector Rotation;
    public string Name;
    public void Set(WrappedBehaviour Beh)
    {
        CheckNull();
        Beh.transform.position = Position;
        Beh.transform.rotation = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
        Beh.name = Name;
    }
    public void Get(WrappedBehaviour Beh){
        Position =  Beh.transform.position;
        Rotation = Beh.transform.rotation;
        Name = Beh.name;
    }
    public void Set(MonoBehaviour Beh)
    {
        CheckNull();
        Beh.transform.position = Position;
        Beh.transform.rotation = Quaternion.Euler(Rotation.x, Rotation.y, Rotation.z);
        Beh.name = Name;
    }
    public void Get(MonoBehaviour Beh){
        Name = Beh.name;
        Position =  Beh.transform.position;
        Rotation = Beh.transform.rotation;
    }

    public void CheckNull()
    {
        Position ??= new Vector(0, 0, 0);
        Rotation ??= new Vector(0, 0, 0);
        Name ??= "";
    }
}