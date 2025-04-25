using System;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDGraphicSettingsRenderer : HUDComponent, IHUDSettingsContentGroup, IView, IDisposable
{
	[SerializeField]
	private PrefabViewReference<HUDSettingRenderer> UISettingRendererPrefab;

	[SerializeField]
	private PrefabViewReference<HUDGenericSettingsHeading> UIHeaderPrefab;

	[SerializeField]
	private PrefabViewReference<HUDGraphicSettingsApplyButtons> UIApplyButtonsPrefab;

	[SerializeField]
	private PrefabViewReference<HUDGraphicSettingsPresetsChooser> UIGraphicSettingsPresetsChooserPrefab;

	[SerializeField]
	private RectTransform UISettingsParent;

	private HUDGraphicSettingsApplyButtons UIDisplayApplyButtons;

	private DisplaySettings PendingDisplaySettings;

	private IHUDDialogStack DialogStack;

	private bool HasPendingDisplaySettings => !PendingDisplaySettings.Equals(Globals.Settings.Display);

	public bool TryLeave()
	{
		if (HasPendingDisplaySettings)
		{
			ShowLeaveDialog();
			return false;
		}
		return true;
	}

	private void ShowLeaveDialog()
	{
		HUDDialogSimpleConfirm dialog = DialogStack.ShowUIDialog<HUDDialogSimpleConfirm>();
		dialog.InitDialogContents("menu.settings.graphics.pending-leave-dialog.title".tr(), "menu.settings.graphics.pending-leave-dialog.description".tr(), "global.btn-apply".tr(), "global.btn-cancel".tr(), HUDTheme.ButtonColorsActive);
		dialog.OnConfirmed.AddListener(OnDisplaySettingsApplyRequested);
	}

	[Construct]
	private void Construct(IHUDDialogStack dialogStack)
	{
		DialogStack = dialogStack;
		PendingDisplaySettings = new DisplaySettings(saveOnChange: false);
		PendingDisplaySettings.CopyFrom(Globals.Settings.Display);
		PendingDisplaySettings.Changed.AddListener(OnPendingDisplaySettingsChanged);
		Globals.Settings.Display.Changed.AddListener(OnPendingDisplaySettingsChanged);
		RenderDisplaySettings(PendingDisplaySettings);
		RenderQualitySettings(Globals.Settings.Graphics);
		UIDisplayApplyButtons.ApplyRequested.AddListener(OnDisplaySettingsApplyRequested);
		UIDisplayApplyButtons.RevertRequested.AddListener(OnDisplaySettingsRevertRequested);
		OnPendingDisplaySettingsChanged();
	}

	protected override void OnDispose()
	{
		PendingDisplaySettings.Changed.RemoveListener(OnPendingDisplaySettingsChanged);
		Globals.Settings.Display.Changed.RemoveListener(OnPendingDisplaySettingsChanged);
		UIDisplayApplyButtons.ApplyRequested.RemoveListener(OnDisplaySettingsApplyRequested);
		UIDisplayApplyButtons.RevertRequested.RemoveListener(OnDisplaySettingsRevertRequested);
	}

	private void RenderDisplaySettings(DisplaySettings settings)
	{
		HUDGenericSettingsHeading header = RequestChildView(UIHeaderPrefab).PlaceAt(UISettingsParent);
		header.Text = settings.Title;
		UIDisplayApplyButtons = RequestChildView(UIApplyButtonsPrefab).PlaceAt(UISettingsParent);
		UIDisplayApplyButtons.Active = false;
		RenderGroupContents(settings);
	}

	private void RenderQualitySettings(GraphicSettings settings)
	{
		HUDGenericSettingsHeading header = RequestChildView(UIHeaderPrefab).PlaceAt(UISettingsParent);
		header.Text = settings.Title;
		RequestChildView(UIGraphicSettingsPresetsChooserPrefab).PlaceAt(UISettingsParent);
		RenderGroupContents(settings);
	}

	private void RenderGroupContents(GameSettingsGroup group)
	{
		foreach (GameSetting setting in group.Settings)
		{
			HUDSettingRenderer instance = RequestChildView(UISettingRendererPrefab).PlaceAt(UISettingsParent);
			instance.Setting = setting;
		}
		if (group.Settings.Count % 2 != 0)
		{
			LayoutUtils.AddGridFillerCell(UISettingsParent);
		}
	}

	private void OnDisplaySettingsApplyRequested()
	{
		PendingDisplaySettings.Apply();
		int countdownSeconds = 15;
		string countdownBaseText = "menu.settings.graphics.confirm-dialog.description".tr();
		HUDDialogConfirmCountdown dialog = DialogStack.ShowUIDialog<HUDDialogConfirmCountdown>();
		dialog.InitDialogContents("menu.settings.graphics.confirm-dialog.title".tr(), countdownBaseText.Replace("<countdown>", StringFormatting.FormatDurationSeconds(countdownSeconds)), countdownSeconds, "menu.settings.graphics.apply".tr(), "menu.settings.graphics.revert".tr());
		dialog.OnCancelled.AddListener(delegate
		{
			PendingDisplaySettings.CopyFrom(Globals.Settings.Display);
			PendingDisplaySettings.Apply();
		});
		dialog.OnConfirmed.AddListener(delegate
		{
			Globals.Settings.Display.CopyFrom(PendingDisplaySettings);
		});
		dialog.OnCountdown.AddListener(delegate(int seconds)
		{
			string newValue = StringFormatting.FormatDurationSeconds(seconds);
			dialog.UIDescription.text = countdownBaseText.Replace("<countdown>", newValue);
		});
	}

	private void OnDisplaySettingsRevertRequested()
	{
		PendingDisplaySettings.CopyFrom(Globals.Settings.Display);
	}

	private void OnPendingDisplaySettingsChanged()
	{
		UIDisplayApplyButtons.Active = HasPendingDisplaySettings;
	}
}
