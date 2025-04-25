using System;
using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDKeybindingsRenderer : HUDComponent, IRunnableView, IView, IHUDSettingsContentGroup, IDisposable
{
	[SerializeField]
	private PrefabViewReference<HUDKeybindingRenderer> UIKeybindingRendererPrefab;

	[SerializeField]
	private PrefabViewReference<HUDGenericSettingsHeading> UIHeaderPrefab;

	[SerializeField]
	private PrefabViewReference<HUDSettingsResetButton> UISettingsResetButtonPrefab;

	[SerializeField]
	private RectTransform UIKeybindingsParent;

	private Dictionary<KeybindingsLayer, HUDSettingsResetButton> ResetButtons = new Dictionary<KeybindingsLayer, HUDSettingsResetButton>();

	public void Run()
	{
		Keybindings keybindings = Globals.Keybindings;
		foreach (KeybindingsLayer layer in keybindings.Layers)
		{
			HUDGenericSettingsHeading header = RequestChildView(UIHeaderPrefab).PlaceAt(UIKeybindingsParent);
			header.Text = layer.Title;
			HUDSettingsResetButton resetButton = RequestChildView(UISettingsResetButtonPrefab).PlaceAt(UIKeybindingsParent);
			ResetButtons[layer] = resetButton;
			resetButton.ResetRequested.AddListener(delegate
			{
				ResetLayer(layer);
			});
			foreach (Keybinding keybinding in layer.Bindings)
			{
				HUDKeybindingRenderer instance = RequestChildView(UIKeybindingRendererPrefab).PlaceAt(UIKeybindingsParent);
				instance.Keybinding = keybinding;
			}
			if (layer.Bindings.Count % 2 != 0)
			{
				LayoutUtils.AddGridFillerCell(UIKeybindingsParent);
			}
		}
		UpdateResetButtons();
	}

	public bool TryLeave()
	{
		return true;
	}

	private void ResetLayer(KeybindingsLayer layer)
	{
		layer.Reset();
	}

	[Construct]
	private void Construct()
	{
		foreach (KeybindingsLayer layer in Globals.Keybindings.Layers)
		{
			layer.Changed.AddListener(UpdateResetButtons);
		}
	}

	protected void UpdateResetButtons()
	{
		foreach (KeyValuePair<KeybindingsLayer, HUDSettingsResetButton> entry in ResetButtons)
		{
			entry.Value.Active = entry.Key.Modified;
		}
	}

	protected override void OnDispose()
	{
		foreach (KeybindingsLayer layer in Globals.Keybindings.Layers)
		{
			layer.Changed.RemoveListener(UpdateResetButtons);
		}
	}
}
