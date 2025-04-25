using UnityEngine;

public abstract class HUDSidePanelModule
{
	protected GameObject ContentContainer;

	public GameObject GetUIPrefab()
	{
		string resourcePath = GetType().Name + "Prefab";
		GameObject result = Resources.Load<GameObject>(resourcePath);
		if (result == null)
		{
			Debug.LogError("Unable to find prefab for info module at: " + resourcePath);
		}
		return result;
	}

	public virtual void OnGameUpdate(InputDownstreamContext context)
	{
	}

	public virtual void Setup(GameObject container)
	{
		ContentContainer = container;
	}

	public virtual void Cleanup()
	{
	}
}
