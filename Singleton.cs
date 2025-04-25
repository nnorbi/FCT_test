using UnityEngine;

[DefaultExecutionOrder(-1)]
public class Singleton<T> : MonoBehaviour where T : Component
{
	public static T G;

	private static T _Instance
	{
		get
		{
			return G;
		}
		set
		{
			G = value;
		}
	}

	public static bool HasInstance => _Instance != null;

	public virtual void Awake()
	{
		if ((bool)_Instance)
		{
			Debug.LogError("There is more than one " + typeof(T).Name + " singleton in the scene!");
		}
		else
		{
			_Instance = this as T;
		}
	}

	protected virtual void OnDestroy()
	{
		_Instance = null;
	}
}
