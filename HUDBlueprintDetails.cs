using System.Collections.Generic;
using Core.Dependency;
using UnityEngine;

public abstract class HUDBlueprintDetails<TBlueprint> : HUDPartWithSidePanel where TBlueprint : class, IBlueprint
{
	private PlayerActionManager PlayerActionManager;

	protected abstract GameScope OperatingScope { get; }

	protected TBlueprint Blueprint => Player.CurrentBlueprint.Value as TBlueprint;

	[Construct]
	private void Construct(PlayerActionManager playerActionManager)
	{
		PlayerActionManager = playerActionManager;
		Player.CurrentBlueprint.Changed.AddListener(OnBlueprintChanged);
	}

	protected override void OnDispose()
	{
		Player.CurrentBlueprint.Changed.RemoveListener(OnBlueprintChanged);
		base.OnDispose();
	}

	private void OnBlueprintChanged(IBlueprint blueprint)
	{
		if (blueprint is TBlueprint)
		{
			SidePanel_MarkDirty();
		}
	}

	protected override string SidePanel_GetTitle()
	{
		return "blueprint-details.title".tr();
	}

	protected override bool SidePanel_ShouldShow()
	{
		return Player.CurrentBlueprint.Value is TBlueprint && Player.Viewport.Scope == OperatingScope;
	}

	protected void SelectBlueprint(IBlueprint blueprint)
	{
		ActionSelectBlueprint action = new ActionSelectBlueprint(Player, blueprint);
		if (PlayerActionManager.TryScheduleAction(action))
		{
			Events.ClearPlayerSelection.Invoke();
			return;
		}
		Globals.UISounds.PlayError();
		Observable<IBlueprint> currentBlueprint = Player.CurrentBlueprint;
		Debug.LogWarning("Failed to select blueprint, action not possible, current = " + currentBlueprint.ToString());
	}

	protected HUDSidePanelModuleStats SidePanelModule_Stats(IEnumerable<HUDSidePanelModuleBaseStat> stats)
	{
		return new HUDSidePanelModuleStats(stats);
	}

	protected HUDSidePanelModuleGenericButton SidePanelModule_CopyToClipboard()
	{
		return new HUDSidePanelModuleGenericButton("blueprint-details.copy-to-clipboard".tr(), delegate
		{
			string serialized = (GUIUtility.systemCopyBuffer = BlueprintSerializer.Serialize(Blueprint));
			BlueprintSerializer.Deserialize(serialized);
		});
	}

	protected HUDSidePanelHotkeyInfoData HotkeyAction_Mirror()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "blueprint-details.mirror-blueprint.title",
			DescriptionId = "blueprint-details.mirror-blueprint.description",
			IconId = "mirror",
			KeybindingId = "building-placement.mirror",
			Handler = Mirror,
			ActiveIf = () => Blueprint.Mirrorable
		};
		void Mirror()
		{
			if (!Blueprint.Mirrorable)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				SelectBlueprint(Blueprint.GenerateMirroredVariantYAxis());
			}
		}
	}

	protected HUDSidePanelHotkeyInfoData HotkeyAction_MirrorInverse()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "blueprint-details.mirror-blueprint-inverse.title",
			DescriptionId = "blueprint-details.mirror-blueprint-inverse.description",
			IconId = "mirror",
			KeybindingId = "building-placement.mirror-inverse",
			Handler = MirrorInverse,
			ActiveIf = () => Blueprint.Mirrorable
		};
		void MirrorInverse()
		{
			if (!Blueprint.Mirrorable)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				SelectBlueprint(Blueprint.GenerateMirroredVariantXAxis());
			}
		}
	}

	protected HUDSidePanelHotkeyInfoData HotkeyAction_SaveToLibrary()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "blueprint-details.add-library.title",
			DescriptionId = "blueprint-details.add-library.description",
			IconId = "save-to-blueprint-library",
			KeybindingId = "building-placement.save-blueprint",
			Handler = SaveToLibrary
		};
		void SaveToLibrary()
		{
			if (Player.CurrentBlueprint == null)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				Events.RequestAddBlueprintToLibrary.Invoke(Blueprint);
			}
		}
	}

	protected HUDSidePanelHotkeyInfoData HotkeyAction_RotateCW()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-cw.title",
			DescriptionId = "placement.rotate-cw.description",
			IconId = "rotate-cw",
			KeybindingId = "building-placement.rotate-cw",
			Handler = RotateCW
		};
		void RotateCW()
		{
			if (Player.CurrentBlueprint == null)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				SelectBlueprint(Player.CurrentBlueprint.Value.GenerateRotatedVariant(Grid.Direction.Bottom));
			}
		}
	}

	protected HUDSidePanelHotkeyInfoData HotkeyAction_RotateCCW()
	{
		return new HUDSidePanelHotkeyInfoData
		{
			TitleId = "placement.rotate-ccw.title",
			DescriptionId = "placement.rotate-ccw.description",
			IconId = "rotate-ccw",
			KeybindingId = "building-placement.rotate-ccw",
			Handler = RotateCCW
		};
		void RotateCCW()
		{
			if (Player.CurrentBlueprint == null)
			{
				Globals.UISounds.PlayError();
			}
			else
			{
				SelectBlueprint(Player.CurrentBlueprint.Value.GenerateRotatedVariant(Grid.Direction.Top));
			}
		}
	}
}
