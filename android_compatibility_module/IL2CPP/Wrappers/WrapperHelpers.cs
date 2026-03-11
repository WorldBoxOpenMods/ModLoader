
using System.Linq.Expressions;
using System.Reflection;
using NeoModLoader.AndroidCompatibilityModule;
using UnityEngine;

public class ObjectPoolGenericMono<T> where T : WrappedBehaviour
{
    private readonly List<T> _elements_total = new List<T>();

	public readonly Queue<T> _elements_inactive = new Queue<T>();

	private readonly T _prefab;

	private readonly Transform _parent_transform;

	public ObjectPoolGenericMono(T pPrefab, Transform pParentTransform)
	{
		_prefab = pPrefab;
		_parent_transform = pParentTransform;
	}

	public void clear(bool pDisable = true)
	{
		_elements_inactive.Clear();
		sortElements();
		foreach (T item in _elements_total)
		{
			release(item, pDisable);
		}
	}

	private void sortElements()
	{
		_elements_total.Sort((T a, T b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
	}

	public T getFirstActive()
	{
		return _elements_total[0];
	}

	public IReadOnlyList<T> getListTotal()
	{
		return _elements_total;
	}

	public void disableInactive()
	{
		foreach (T item in _elements_inactive)
		{
			if (item.gameObject.activeSelf)
			{
				item.gameObject.SetActive(value: false);
			}
		}
	}

	public T getNext()
	{
		T newOrActivate = getNewOrActivate();
		checkActive(newOrActivate);
		return newOrActivate;
	}

	private T getNewOrActivate()
	{
		T val;
		if (_elements_inactive.Count > 0)
		{
			val = _elements_inactive.Dequeue();
		}
		else
		{
			val = WrapperHelper.Instantiate(_prefab, _parent_transform);
			_elements_total.Add(val);
			val.name = typeof(T)?.ToString() + " " + _elements_total.Count + " " + val.transform.GetSiblingIndex();
		}
		return val;
	}

	public void release(T pElement, bool pDisable = true)
	{
		if (_parent_transform.gameObject.activeInHierarchy)
		{
			pElement.transform.SetAsLastSibling();
		}
		if (!_elements_inactive.Contains(pElement))
		{
			_elements_inactive.Enqueue(pElement);
		}
		if (pElement.gameObject.activeSelf && pDisable)
		{
			pElement.gameObject.SetActive(value: false);
		}
	}

	private void checkActive(T pElement)
	{
		if (!pElement.gameObject.activeSelf)
		{
			pElement.gameObject.SetActive(value: true);
		}
	}

	public int countTotal()
	{
		return _elements_total.Count;
	}

	public int countInactive()
	{
		return _elements_inactive.Count;
	}

	public int countActive()
	{
		return _elements_total.Count - _elements_inactive.Count;
	}

	public void resetParent()
	{
		foreach (T item in _elements_total)
		{
			resetParent(item);
		}
	}

	public void resetParent(T pElement)
	{
		if (_parent_transform.gameObject.activeInHierarchy)
		{
			pElement.transform.SetParent(_parent_transform);
		}
	}
}

public static class WrapperHelper
{
	public static WrappedAction CreateWrappedAction(MethodInfo method)
	{
		var param = Expression.Parameter(typeof(WrappedBehaviour), "beh");

		var call = Expression.Call(
			Expression.Convert(param, method.DeclaringType), // cast WrappedBehaviour → ConcurrentLogHandle
			method
		);
		return Expression.Lambda<WrappedAction>(call, param).Compile();
	}
	public static T Instantiate<T>(T original, Transform parent, bool worldPositionStays = true) where T : WrappedBehaviour
	{ 
		Il2CPPBehaviour il2cpp = UnityEngine.Object.Instantiate(original.Wrapper, parent, worldPositionStays);
		WrapperResolver.ResolveInstantiate(original.Wrapper.gameObject, il2cpp.gameObject);
		return (T)il2cpp.WrappedBehaviour;
	}
	public static GameObject Instantiate(GameObject original, Transform parent, bool worldPositionStays = true)
	{ 
		GameObject newobj = UnityEngine.Object.Instantiate(original, parent, worldPositionStays);
		WrapperResolver.ResolveInstantiate(original, newobj);
		return newobj;
	}
	public static object GetWrappedComponent(GameObject Object, Type WrappedType)
	{
		foreach (Il2CPPBehaviour beh in Object.GetComponents<Il2CPPBehaviour>())
		{
			if (beh.WrappedBehaviour == null)
			{
				continue;
			}

			if (beh.WrappedType.IsAssignableTo(WrappedType))
			{
				return beh.WrappedBehaviour;
			}
		}
		return null;
	}
}

public class WrapperResolver : IDisposable
{
	static void AddChildren(Transform transform, List<Transform> children)
	{
		foreach (Transform child in transform.GetChildren())
		{
			children.Add(child);
			AddChildren(child, children);
		}
	}
	List<Transform> OrigObjects;
	List<Transform> ClonedObjects;

	public static void ResolveInstantiate(GameObject orig, GameObject clone)
	{
		WrapperResolver resolver = new WrapperResolver(orig, clone);
		resolver.Resolve();
		resolver.Dispose();
	}
	public WrapperResolver(GameObject orig, GameObject clone)
	{
		OrigObjects = new List<Transform>{orig.transform};
		ClonedObjects = new List<Transform>{clone.transform};
		AddChildren(orig.transform, OrigObjects);
		AddChildren(clone.transform, ClonedObjects);
	}

	public void Resolve()
	{
		for (int i = 0; i < OrigObjects.Count; i++)
		{
			Il2CPPBehaviour[] origbeh = OrigObjects[i].GetComponents<Il2CPPBehaviour>();
			if (origbeh == null) continue;
			Il2CPPBehaviour[] clonedbeh =  ClonedObjects[i].GetComponents<Il2CPPBehaviour>();
			for (int j = 0; j < origbeh.Length; j++)
			{
				Clone(origbeh[j], clonedbeh[j]);
			}
		}
	}
	static int Getindex(Component beh, GameObject obj)
	{
		var arr = obj.GetComponents(beh.GetType().C());
		int result = arr.GetIndex(beh);
		return result;
	}
	public void Clone(Il2CPPBehaviour orig, Il2CPPBehaviour clone)
	{
		WrappedBehaviour beh = orig.WrappedBehaviour;
		Type WrappedType = orig.WrappedType;
		WrappedBehaviour cloned = clone.CreateWrapperIfNull(WrappedType);
		var fields = WrappedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
		foreach (var field in fields)
		{
			Type type = field.FieldType;
			if (field.GetValue(beh) == null)
			{
				continue;
			}
			if (type == typeof(GameObject))
			{
				var obj =  (GameObject)field.GetValue(beh);
				field.SetValue(cloned, ResolveGameObject(obj));
			}
			else if (type== typeof(Transform))
			{
				var obj =  (Transform)field.GetValue(beh);
				field.SetValue(cloned, ResolveGameObject(obj.gameObject).transform);
			}
			else if (typeof(Component).IsAssignableFrom(type))
			{
				var obj =  (Component)field.GetValue(beh);
				field.SetValue(cloned, ResolveGameObject(obj.gameObject).GetComponent(type, Getindex(obj, obj.gameObject)));
			}
			else if (typeof(WrappedBehaviour).IsAssignableFrom(type))
			{
				var obj =  (WrappedBehaviour)field.GetValue(beh);
				field.SetValue(cloned, ((Il2CPPBehaviour)ResolveGameObject(obj.gameObject).GetComponent(typeof(Il2CPPBehaviour), Getindex(obj.Wrapper, obj.gameObject))).CreateWrapperIfNull(type));
			}
			else
			{
				field.SetValue(cloned, field.GetValue(beh));
			}
		}
	}
	public GameObject ResolveGameObject(GameObject orig)
	{
		if (!OrigObjects.Contains(orig.transform))
		{
			return orig;
		}
		return ClonedObjects[OrigObjects.IndexOf(orig.transform)].gameObject;
	}

	public void Dispose()
	{
		OrigObjects = null;
		ClonedObjects = null;
	}
}