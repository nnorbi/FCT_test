using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPins
{
	public class SerializedData
	{
		public string[] PinnedNodes = new string[0];
	}

	public UnityEvent OnChanged = new UnityEvent();

	public UnityEvent<IResearchableHandle> OnResearchableAdded = new UnityEvent<IResearchableHandle>();

	public UnityEvent<IResearchableHandle> OnResearchableRemoved = new UnityEvent<IResearchableHandle>();

	private List<IResearchableHandle> _PinnedResearchables = new List<IResearchableHandle>();

	public IReadOnlyList<IResearchableHandle> PinnedResearchables => _PinnedResearchables;

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			PinnedNodes = _PinnedResearchables.Select((IResearchableHandle n) => n.Meta.name).ToArray()
		};
	}

	public bool IsPinned(IResearchableHandle researchable)
	{
		return _PinnedResearchables.Contains(researchable);
	}

	public bool TryPin(IResearchableHandle researchable)
	{
		if (IsPinned(researchable))
		{
			return false;
		}
		_PinnedResearchables.Add(researchable);
		OnResearchableAdded.Invoke(researchable);
		OnChanged.Invoke();
		return true;
	}

	public bool TryUnpin(IResearchableHandle researchable)
	{
		if (!IsPinned(researchable))
		{
			return false;
		}
		int index = _PinnedResearchables.IndexOf(researchable);
		_PinnedResearchables.RemoveAt(index);
		OnResearchableRemoved.Invoke(researchable);
		OnChanged.Invoke();
		return true;
	}

	public bool TogglePinned(IResearchableHandle researchable)
	{
		if (IsPinned(researchable))
		{
			return TryUnpin(researchable);
		}
		return TryPin(researchable);
	}

	public void Deserialize(SerializedData data)
	{
		foreach (IResearchableHandle researchable in _PinnedResearchables)
		{
			OnResearchableRemoved.Invoke(researchable);
		}
		_PinnedResearchables.Clear();
		if (data.PinnedNodes == null)
		{
			Debug.Log("Pins:: Pinned nodes = null");
			OnChanged.Invoke();
			return;
		}
		string[] pinnedNodes = data.PinnedNodes;
		foreach (string researchableId in pinnedNodes)
		{
			IResearchableHandle researchable2 = Singleton<GameCore>.G.Research.FindResearchableById(researchableId);
			if (researchable2 == null)
			{
				Debug.LogWarning("Pins:: Failed to find node by id: " + researchableId);
				continue;
			}
			_PinnedResearchables.Add(researchable2);
			OnResearchableAdded.Invoke(researchable2);
		}
		OnChanged.Invoke();
	}
}
