using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDKeybindingsHelper : HUDPart
{
	[SerializeField]
	private PrefabViewReference<HUDContextualHotkeyActions> UIHotkeyActionsPrefab;

	private HUDContextualHotkeyActions UIHotkeyActions;

	[Construct]
	private void Construct()
	{
		UIHotkeyActions = RequestChildView(UIHotkeyActionsPrefab).PlaceAt(base.transform);
		UpdateActions();
		Player.Viewport.ScopeChanged.AddListener(UpdateActions);
	}

	private void UpdateActions()
	{
		UIHotkeyActions.SetActions(GetActions());
	}

	protected override void OnDispose()
	{
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (context.IsTokenAvailable("HUDPart$context_actions") && Player.Viewport.Scope != GameScope.Overview && Player.Viewport.Scope != GameScope.Trains)
		{
			UIHotkeyActions.Show();
		}
		else
		{
			UIHotkeyActions.Hide();
		}
		base.OnUpdate(context);
	}

	private IEnumerable<HUDSidePanelHotkeyInfoData> GetActions()
	{
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "keybinding-helpers.pipette.title",
			DescriptionId = "keybinding-helpers.pipette.description",
			IconId = "pipette",
			KeybindingId = "mass-selection.pipette"
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "keybinding-helpers.delete-drag.title",
			DescriptionId = "keybinding-helpers.delete-drag.description",
			IconId = "delete",
			KeybindingId = "mass-selection.quick-delete-drag",
			KeybindingIsToggle = true
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "keybinding-helpers.area-select.title",
			DescriptionId = "keybinding-helpers.area-select.description",
			IconId = "area-select-add",
			KeybindingId = "mass-selection.select-area-modifier",
			KeybindingIsToggle = true,
			AdditionalKeybindingId = "mass-selection.select-base"
		};
		yield return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "keybinding-helpers.area-delete.title",
			DescriptionId = "keybinding-helpers.area-delete.description",
			IconId = "area-select-delete",
			KeybindingId = "mass-selection.select-area-modifier",
			KeybindingIsToggle = true,
			AdditionalKeybindingId = "mass-selection.quick-delete-drag"
		};
	}
}
