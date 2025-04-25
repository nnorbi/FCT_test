using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

[Serializable]
public class KeybindingsLayer
{
	public string Id;

	public List<Keybinding> Bindings;

	public readonly UnityEvent Changed = new UnityEvent();

	public string Title => ("keybindings." + Id).tr();

	public bool Modified => Bindings.Any((Keybinding k) => k.Modified);

	public KeybindingsLayer(string id, Keybinding[] mappings)
	{
		Id = id;
		Bindings = mappings.ToList();
		foreach (Keybinding binding in mappings)
		{
			binding.Changed.AddListener(Changed.Invoke);
		}
	}

	public void Reset()
	{
		foreach (Keybinding keybinding in Bindings)
		{
			keybinding.Reset();
		}
	}
}
