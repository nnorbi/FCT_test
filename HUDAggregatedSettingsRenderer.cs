using System;
using System.Collections.Generic;
using Unity.Core.View;
using UnityEngine;

public abstract class HUDAggregatedSettingsRenderer : HUDComponent, IHUDSettingsContentGroup, IView, IDisposable
{
	[SerializeField]
	private PrefabViewReference<HUDSettingRenderer> UISettingRendererPrefab;

	[SerializeField]
	private PrefabViewReference<HUDGenericSettingsHeading> UIHeaderPrefab;

	[SerializeField]
	private PrefabViewReference<HUDSettingsResetButton> UISettingsResetButtonPrefab;

	[SerializeField]
	private RectTransform UISettingsParent;

	private Dictionary<GameSettingsGroup, HUDSettingsResetButton> ResetButtons = new Dictionary<GameSettingsGroup, HUDSettingsResetButton>();

	public abstract bool TryLeave();

	protected void RenderGroup(GameSettingsGroup group, bool allowReset)
	{
		HUDGenericSettingsHeading header = RequestChildView(UIHeaderPrefab).PlaceAt(UISettingsParent);
		header.Text = group.Title;
		if (allowReset)
		{
			HUDSettingsResetButton resetButton = RequestChildView(UISettingsResetButtonPrefab).PlaceAt(UISettingsParent);
			resetButton.ResetRequested.AddListener(delegate
			{
				ResetGroup(group);
			});
			ResetButtons[group] = resetButton;
			UpdateResetButton(group, resetButton);
		}
		else
		{
			LayoutUtils.AddGridFillerCell(UISettingsParent);
		}
		foreach (GameSetting setting in group.Settings)
		{
			if (setting.ShowInUI)
			{
				HUDSettingRenderer instance = RequestChildView(UISettingRendererPrefab).PlaceAt(UISettingsParent);
				instance.Setting = setting;
			}
		}
		if (group.Settings.Count % 2 != 0)
		{
			LayoutUtils.AddGridFillerCell(UISettingsParent);
		}
		group.Changed.AddListener(UpdateResetButtons);
	}

	private void UpdateResetButtons()
	{
		foreach (var (group, button) in ResetButtons)
		{
			UpdateResetButton(group, button);
		}
	}

	private void UpdateResetButton(GameSettingsGroup group, HUDSettingsResetButton button)
	{
		button.Active = group.IsModified;
	}

	private void ResetGroup(GameSettingsGroup group)
	{
		group.ResetToDefault();
	}

	protected override void OnDispose()
	{
		foreach (GameSettingsGroup group in ResetButtons.Keys)
		{
			group.Changed.RemoveListener(UpdateResetButtons);
		}
	}
}
