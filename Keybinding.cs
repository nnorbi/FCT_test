using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Keybinding
{
	public const int KEY_SET_COUNT = 2;

	public readonly UnityEvent Changed = new UnityEvent();

	protected KeySet[] DefaultKeySets = new KeySet[2];

	protected KeySet[] KeySets = new KeySet[2];

	public string Id { get; private set; }

	public string PartialId { get; private set; }

	public bool IsModifierOnly { get; private set; }

	public bool SystemDefined { get; private set; }

	public bool GenerallyBlockableByUI { get; private set; }

	public float AxisThreshold { get; private set; }

	public string Title
	{
		get
		{
			string translation = ("keybinding." + Id).tr();
			if (IsModifierOnly)
			{
				return "keybindings.with-modifier".tr(("<original>", translation));
			}
			return translation;
		}
	}

	public bool Modified
	{
		get
		{
			for (int i = 0; i < KeySets.Length; i++)
			{
				if (IsKeySetOverridden(i))
				{
					return true;
				}
			}
			return false;
		}
	}

	public Keybinding(string partialId, KeySet defaultKeySet, KeySet? secondaryKeySet = null, bool blockableByUI = true, bool systemDefined = false, float axisThreshold = 0.001f, bool isModifierOnly = false)
	{
		AxisThreshold = axisThreshold;
		PartialId = partialId;
		SystemDefined = systemDefined;
		DefaultKeySets[0] = defaultKeySet;
		DefaultKeySets[1] = secondaryKeySet ?? KeySet.EMPTY;
		KeySets[0] = defaultKeySet;
		KeySets[1] = secondaryKeySet ?? KeySet.EMPTY;
		GenerallyBlockableByUI = blockableByUI;
		IsModifierOnly = isModifierOnly;
	}

	public void Save()
	{
		if (string.IsNullOrEmpty(Id))
		{
			throw new Exception("Can't load keybinding without id");
		}
		for (int i = 0; i < KeySets.Length; i++)
		{
			KeySets[i].Save(Id + "." + i, DefaultKeySets[i]);
		}
		Debug.Log("Save keybinding " + Id);
	}

	public void Load()
	{
		if (string.IsNullOrEmpty(Id))
		{
			throw new Exception("Can't load keybinding without id");
		}
		for (int i = 0; i < KeySets.Length; i++)
		{
			KeySets[i].Load(Id + "." + i, DefaultKeySets[i]);
		}
		Changed.Invoke();
	}

	public void OverrideKeySet(int index, KeySet keySet)
	{
		KeySets[index] = keySet;
		Changed.Invoke();
	}

	public KeySet GetDefaultKeySetAt(int index)
	{
		return DefaultKeySets[index];
	}

	public bool IsKeySetOverridden(int index)
	{
		return !KeySets[index].Equals(DefaultKeySets[index]);
	}

	public void ResetKeySet(int index)
	{
		OverrideKeySet(index, DefaultKeySets[index]);
	}

	public void Reset()
	{
		for (int i = 0; i < 2; i++)
		{
			ResetKeySet(i);
		}
		Save();
	}

	public KeySet GetKeySetAt(int index)
	{
		return KeySets[index];
	}

	public void AssignIdAndLoad(string id)
	{
		Id = id;
		Load();
	}

	public bool IsAnySetActive()
	{
		return KeySets.Any((KeySet keySet) => keySet.IsCurrentlyActive(AxisThreshold));
	}
}
