using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class ResearchProgress
{
	public class SerializedData
	{
		public string[] UnlockedNodes = new string[0];
	}

	private ResearchTreeHandle Tree;

	public UnityEvent<MetaResearchable> OnResearchableUnlocked = new UnityEvent<MetaResearchable>();

	public SafeEvent OnChanged = new SafeEvent();

	private HashSet<IResearchUnlock> UnlockedUnlocks = new HashSet<IResearchUnlock>();

	private bool OngoingBulkOperation = false;

	private HashSet<MetaResearchable> _UnlockedResearchables = new HashSet<MetaResearchable>();

	public IReadOnlyCollection<MetaResearchable> UnlockedResearchables => _UnlockedResearchables;

	public ResearchProgress(ResearchTreeHandle tree)
	{
		Tree = tree;
	}

	public float ComputeProgress()
	{
		return math.pow(math.saturate((float)_UnlockedResearchables.Count / (float)math.max(1, Tree.AllResearchables.Length)), 1.7f);
	}

	public bool IsUnlocked(MetaResearchable researchable)
	{
		return _UnlockedResearchables.Contains(researchable);
	}

	public bool IsUnlocked(IResearchableHandle researchable)
	{
		return _UnlockedResearchables.Contains(researchable.Meta);
	}

	public bool IsUnlocked(IResearchUnlock unlock)
	{
		return UnlockedUnlocks.Contains(unlock);
	}

	public void Unlock(IResearchableHandle researchable)
	{
		Unlock(researchable.Meta);
	}

	public void Unlock(MetaResearchable researchable)
	{
		if (IsUnlocked(researchable))
		{
			return;
		}
		_UnlockedResearchables.Add(researchable);
		foreach (IResearchUnlock unlock in researchable.Unlocks)
		{
			UnlockedUnlocks.Add(unlock);
		}
		OnResearchableUnlocked.Invoke(researchable);
		if (!OngoingBulkOperation)
		{
			OnChanged.Invoke();
		}
	}

	public void Lock(IResearchableHandle researchable)
	{
		Lock(researchable.Meta);
	}

	public void Lock(MetaResearchable researchable)
	{
		if (!IsUnlocked(researchable))
		{
			return;
		}
		_UnlockedResearchables.Remove(researchable);
		foreach (IResearchUnlock unlock in researchable.Unlocks)
		{
			UnlockedUnlocks.Remove(unlock);
		}
		if (!OngoingBulkOperation)
		{
			OnChanged.Invoke();
		}
	}

	public void BulkOperation(Action action)
	{
		if (OngoingBulkOperation)
		{
			throw new Exception("Double mass change is not allowed");
		}
		OngoingBulkOperation = true;
		try
		{
			action();
		}
		finally
		{
			OngoingBulkOperation = false;
			OnChanged.Invoke();
		}
	}

	public void LockBulk(IEnumerable<IResearchableHandle> researchables)
	{
		BulkOperation(delegate
		{
			foreach (IResearchableHandle current in researchables)
			{
				Lock(current);
			}
		});
	}

	public void UnlockBulk(IEnumerable<IResearchableHandle> researchables)
	{
		BulkOperation(delegate
		{
			foreach (IResearchableHandle current in researchables)
			{
				Unlock(current);
			}
		});
	}

	public SerializedData Serialize()
	{
		return new SerializedData
		{
			UnlockedNodes = _UnlockedResearchables.Select((MetaResearchable researchable) => researchable.name).ToArray()
		};
	}

	public void Deserialize(SerializedData data)
	{
		List<MetaResearchable> researchables = (from researchable in data.UnlockedNodes.Select(delegate(string name)
			{
				IResearchableHandle researchableHandle = Array.Find(Tree.AllResearchables, (IResearchableHandle n) => n.Meta.name == name);
				if (researchableHandle == null)
				{
					Debug.LogWarning("Node '" + name + "' not found!");
					return (MetaResearchable)null;
				}
				return researchableHandle.Meta;
			})
			where researchable != null
			select researchable).ToList();
		Debug.Log("Research:: Deserialize " + data.UnlockedNodes.Length + " unlocked nodes");
		BulkOperation(delegate
		{
			MetaResearchable[] array = _UnlockedResearchables.ToArray();
			foreach (MetaResearchable metaResearchable in array)
			{
				if (!researchables.Contains(metaResearchable))
				{
					Lock(metaResearchable);
				}
			}
			foreach (MetaResearchable current in researchables)
			{
				Unlock(current);
			}
		});
	}
}
