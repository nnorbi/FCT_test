#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;

public class HUDScopeManager : HUDPart
{
	private const float ZOOM_BIAS = 0.01f;

	private const string KEYBINDING_SCOPE = "main.scope-change";

	private const string KEYBINDING_TRAIN_MANAGEMENT = "main.toggle-rail-management";

	public static readonly float BUILDINGS_MAX_ZOOM = 70f;

	public static readonly float ISLANDS_MIN_ZOOM = 70f;

	public static readonly float TRAINS_MIN_ZOOM = 40f;

	public static IReadOnlyDictionary<GameScope, float> DEFAULT_ZOOMS = new Dictionary<GameScope, float>
	{
		{
			GameScope.Buildings,
			33f
		},
		{
			GameScope.Overview,
			4000f
		},
		{
			GameScope.Trains,
			190f
		},
		{
			GameScope.Islands,
			190f
		}
	};

	public static readonly float OVERVIEW_ZOOM = 1200f;

	[SerializeField]
	private PrefabViewReference<HUDMainButtonToolbarSlot> UIMainButtonPrefab;

	[SerializeField]
	private RectTransform UIButtonsParent;

	private InterpolatedShaderInput ShaderInputPlacingBuilding;

	private InterpolatedShaderInput ShaderInputPlacingIsland;

	private Dictionary<GameScope, float?> StoredZooms = new Dictionary<GameScope, float?>();

	private IHUDDialogStack DialogStack;

	private PlayerActionManager ActionManager;

	private ResearchManager ResearchManager;

	private GameModeHandle GameMode;

	private float TargetZoom
	{
		get
		{
			return Player.Viewport.TargetZoom;
		}
		set
		{
			Player.Viewport.TargetZoom = value;
		}
	}

	private bool IsStuckInBuildingMode
	{
		get
		{
			IBlueprint value = Player.CurrentBlueprint.Value;
			return (value != null && value.Scope == GameScope.Buildings) || Player.SelectedBuildingVariant != null;
		}
	}

	private bool IsStuckInIslandMode
	{
		get
		{
			IBlueprint value = Player.CurrentBlueprint.Value;
			return (value != null && value.Scope == GameScope.Islands) || Player.SelectedIslandLayout != null;
		}
	}

	private bool IsStuckInTrainsMode => true;

	[Construct]
	private void Construct(PlayerActionManager actionManager, IHUDDialogStack dialogStack, ResearchManager researchManager, GameModeHandle gameMode)
	{
		ActionManager = actionManager;
		ResearchManager = researchManager;
		GameMode = gameMode;
		DialogStack = dialogStack;
		GameScope[] array = (GameScope[])Enum.GetValues(typeof(GameScope));
		foreach (GameScope scope in array)
		{
			StoredZooms[scope] = null;
		}
		ShaderInputPlacingBuilding = new InterpolatedShaderInput(GlobalShaderInputs.PlacingBuilding);
		ShaderInputPlacingIsland = new InterpolatedShaderInput(GlobalShaderInputs.PlacingIsland);
		Player.CurrentBlueprint.Changed.AddListener(OnBlueprintChanged);
		Player.SelectedBuildingVariant.Changed.AddListener(OnBuildingVariantChanged);
		Player.SelectedIslandLayout.Changed.AddListener(OnIslandLayoutChanged);
		AddButton(new HUDMainButtonConfig
		{
			Icon = Globals.Resources.UIGlobalIconMapping.Get("toolbar-category-buildings"),
			Part = this,
			Location = HUDMainButtons.ButtonLocation.Left,
			TooltipHeaderId = "scope-manager.buildings-mode.title",
			KeybindingId = "main.scope-change",
			IsEnabled = () => true,
			IsActive = () => Player.Viewport.Scope == GameScope.Buildings,
			HasBadge = () => false,
			IsVisible = () => true,
			ListenToKeybinding = false,
			OnActivate = UIRequestBuildingScope
		});
		AddButton(new HUDMainButtonConfig
		{
			Icon = Globals.Resources.UIGlobalIconMapping.Get("toolbar-category-islands"),
			Part = this,
			Location = HUDMainButtons.ButtonLocation.Left,
			TooltipHeaderId = "scope-manager.islands-mode.title",
			KeybindingId = "main.scope-change",
			IsEnabled = () => Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.IslandManagementUnlock),
			IsActive = () => Player.Viewport.Scope == GameScope.Islands,
			HasBadge = () => false,
			IsVisible = () => true,
			ListenToKeybinding = false,
			OnActivate = UIRequestIslandScope
		});
		AddButton(new HUDMainButtonConfig
		{
			Icon = Globals.Resources.UIGlobalIconMapping.Get("toolbar-category-trains"),
			Part = this,
			Location = HUDMainButtons.ButtonLocation.Left,
			TooltipHeaderId = "scope-manager.rails-mode.title",
			KeybindingId = "main.toggle-rail-management",
			IsEnabled = () => Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.RailsUnlock),
			IsActive = () => Player.Viewport.Scope == GameScope.Trains,
			HasBadge = () => false,
			IsVisible = () => true,
			ListenToKeybinding = false,
			OnActivate = UIRequestTrainScope
		});
	}

	private void AddButton(HUDMainButtonConfig config)
	{
		HUDMainButtonToolbarSlot view = RequestChildView(UIMainButtonPrefab).PlaceAt(UIButtonsParent);
		view.SetConfig(config);
	}

	protected override void OnDispose()
	{
		Player.CurrentBlueprint.Changed.RemoveListener(OnBlueprintChanged);
		Player.SelectedBuildingVariant.Changed.RemoveListener(OnBuildingVariantChanged);
		Player.SelectedIslandLayout.Changed.RemoveListener(OnIslandLayoutChanged);
	}

	private void UIRequestBuildingScope()
	{
		SwitchIntoScope(GameScope.Buildings, manual: true);
	}

	private void UIRequestIslandScope()
	{
		if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.IslandManagementUnlock))
		{
			Globals.UISounds.PlayError();
		}
		else
		{
			SwitchIntoScope(GameScope.Islands, manual: true);
		}
	}

	private void UIRequestTrainScope()
	{
		if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.RailsUnlock))
		{
			Globals.UISounds.PlayError();
		}
		else
		{
			SwitchIntoScope(GameScope.Trains, manual: true);
		}
	}

	private void UIRequestOverviewScope()
	{
		SwitchIntoScope(GameScope.Overview, manual: true);
	}

	private void ClearPlayerSelections()
	{
		base.Logger.Debug?.Log("Clear player selections");
		Player.BuildingSelection.Clear();
		Player.IslandSelection.Clear();
	}

	private void OnBlueprintChanged(IBlueprint blueprint)
	{
		base.Logger.Debug?.Log($"OnBlueprintChanged ({blueprint?.Scope} / {blueprint?.BuildingCount})");
		if (blueprint != null)
		{
			ClearPlayerSelections();
			SwitchIntoScope(blueprint.Scope, manual: true);
		}
	}

	private void OnBuildingVariantChanged(MetaBuildingVariant variant)
	{
		base.Logger.Debug?.Log("OnBuildingVariantChanged(" + variant?.name + ")");
		if (!(variant == null))
		{
			DeselectCurrentBlueprint();
			ClearPlayerSelections();
			SwitchIntoScope(GameScope.Buildings, manual: true);
		}
	}

	private void OnIslandLayoutChanged(MetaIslandLayout layout)
	{
		base.Logger.Debug?.Log("OnBuildingVariantChanged(" + layout?.name + ")");
		if (!(layout == null))
		{
			DeselectCurrentBlueprint();
			ClearPlayerSelections();
			SwitchIntoScope(GameScope.Islands, manual: true);
		}
	}

	private void SwitchIntoScope(GameScope scope, bool manual)
	{
		if (scope != Player.Viewport.Scope)
		{
			base.Logger.Debug?.Log($"Switch from {Player.Viewport.Scope} into {scope} (manual: {manual})");
			if (manual)
			{
				SaveCurrentZoom();
				TargetZoom = StoredZooms[scope] ?? DEFAULT_ZOOMS[scope];
			}
			else
			{
				StoredZooms[Player.Viewport.Scope] = null;
			}
			if (scope != GameScope.Buildings)
			{
				Player.SelectedBuildingVariant.Value = null;
			}
			if (scope != GameScope.Islands)
			{
				Player.SelectedIslandLayout.Value = null;
			}
			if (Player.CurrentBlueprint.Value != null && Player.CurrentBlueprint.Value.Scope != scope)
			{
				DeselectCurrentBlueprint();
			}
			ClearPlayerSelections();
			StoredZooms[scope] = null;
			Player.Viewport.Scope = scope;
			base.Logger.Debug?.Log("> Switch complete.");
		}
	}

	private void DeselectCurrentBlueprint()
	{
		if (Player.CurrentBlueprint.Value != null)
		{
			base.Logger.Debug?.Log("DeselectCurrentBlueprint()");
			ActionSelectBlueprint action = new ActionSelectBlueprint(Player, (IBlueprint)null);
			if (!ActionManager.TryScheduleAction(action))
			{
				Debug.LogError("Failed to clear blueprint for scope");
			}
		}
	}

	private void SaveCurrentZoom()
	{
		switch (Player.Viewport.Scope)
		{
		case GameScope.Buildings:
			StoredZooms[GameScope.Buildings] = math.clamp(Player.Viewport.Zoom, 0f, BUILDINGS_MAX_ZOOM - 0.01f);
			break;
		case GameScope.Islands:
			StoredZooms[GameScope.Islands] = math.clamp(Player.Viewport.Zoom, ISLANDS_MIN_ZOOM + 0.01f, OVERVIEW_ZOOM - 0.01f);
			break;
		case GameScope.Overview:
			StoredZooms[GameScope.Overview] = math.max(Player.Viewport.Zoom, OVERVIEW_ZOOM + 0.01f);
			break;
		case GameScope.Trains:
			StoredZooms[GameScope.Trains] = Player.Viewport.Zoom;
			break;
		}
	}

	private bool RequestedScopeToggle(InputDownstreamContext context)
	{
		return context.ConsumeWasActivated("main.scope-change");
	}

	private void UpdateScope_Buildings(InputDownstreamContext context)
	{
		if (TargetZoom > BUILDINGS_MAX_ZOOM)
		{
			if (IsStuckInBuildingMode)
			{
				TargetZoom = math.min(TargetZoom, BUILDINGS_MAX_ZOOM - 0.01f);
			}
			else
			{
				SwitchIntoScope(GameScope.Islands, manual: false);
			}
		}
		else if (RequestedScopeToggle(context))
		{
			UIRequestIslandScope();
		}
		else if (context.ConsumeWasActivated("main.toggle-rail-management"))
		{
			UIRequestTrainScope();
		}
	}

	private void UpdateScope_Islands(InputDownstreamContext context)
	{
		if (TargetZoom > OVERVIEW_ZOOM)
		{
			if (IsStuckInIslandMode)
			{
				TargetZoom = math.min(TargetZoom, OVERVIEW_ZOOM - 0.01f);
			}
			else
			{
				SwitchIntoScope(GameScope.Overview, manual: false);
			}
		}
		else if (TargetZoom < ISLANDS_MIN_ZOOM)
		{
			if (IsStuckInIslandMode)
			{
				TargetZoom = math.max(TargetZoom, ISLANDS_MIN_ZOOM + 0.01f);
			}
			else
			{
				SwitchIntoScope(GameScope.Buildings, manual: false);
			}
		}
		else if (RequestedScopeToggle(context))
		{
			UIRequestBuildingScope();
		}
		else if (context.ConsumeWasActivated("main.toggle-rail-management"))
		{
			UIRequestTrainScope();
		}
	}

	private void UpdateScope_Trains(InputDownstreamContext context)
	{
		if (TargetZoom > OVERVIEW_ZOOM)
		{
			if (IsStuckInTrainsMode)
			{
				TargetZoom = math.min(TargetZoom, OVERVIEW_ZOOM - 0.01f);
			}
			else
			{
				SwitchIntoScope(GameScope.Overview, manual: false);
			}
		}
		else if (TargetZoom < TRAINS_MIN_ZOOM)
		{
			if (IsStuckInTrainsMode)
			{
				TargetZoom = math.max(TargetZoom, TRAINS_MIN_ZOOM + 0.01f);
			}
			else
			{
				SwitchIntoScope(GameScope.Islands, manual: false);
			}
		}
		else if (RequestedScopeToggle(context))
		{
			UIRequestIslandScope();
		}
		else if (context.ConsumeWasActivated("main.toggle-rail-management"))
		{
			UIRequestIslandScope();
		}
	}

	private void UpdateScope_Overview(InputDownstreamContext context)
	{
		if (TargetZoom < OVERVIEW_ZOOM)
		{
			SwitchIntoScope(GameScope.Islands, manual: false);
		}
		else if (RequestedScopeToggle(context))
		{
			UIRequestIslandScope();
		}
		else if (context.ConsumeWasActivated("main.toggle-rail-management"))
		{
			UIRequestTrainScope();
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		base.OnGameUpdate(context, drawOptions);
		if (context.ConsumeWasActivated("mass-selection.paste-blueprint"))
		{
			HandleBlueprintPaste();
		}
		switch (Player.Viewport.Scope)
		{
		case GameScope.Buildings:
			UpdateScope_Buildings(context);
			break;
		case GameScope.Islands:
			UpdateScope_Islands(context);
			break;
		case GameScope.Trains:
			UpdateScope_Trains(context);
			break;
		case GameScope.Overview:
			UpdateScope_Overview(context);
			break;
		}
		UpdateShaderInputs();
		base.gameObject.SetActiveSelfExt(ResearchManager.Progress.IsUnlocked(GameMode.ResearchConfig.IslandManagementUnlock));
	}

	private void UpdateShaderInputs()
	{
		InterpolatedShaderInput shaderInputPlacingBuilding = ShaderInputPlacingBuilding;
		int num;
		if (Player.Viewport.Scope == GameScope.Buildings)
		{
			IBlueprint value = Player.CurrentBlueprint.Value;
			if ((value != null && value.Scope == GameScope.Buildings) || Player.SelectedBuildingVariant != null)
			{
				num = 1;
				goto IL_0051;
			}
		}
		num = 0;
		goto IL_0051;
		IL_0051:
		shaderInputPlacingBuilding.Update(num);
		InterpolatedShaderInput shaderInputPlacingIsland = ShaderInputPlacingIsland;
		int num2;
		if (Player.Viewport.Scope == GameScope.Islands)
		{
			IBlueprint value2 = Player.CurrentBlueprint.Value;
			if ((value2 != null && value2.Scope == GameScope.Islands) || Player.SelectedIslandLayout != null)
			{
				num2 = 1;
				goto IL_00a9;
			}
		}
		num2 = 0;
		goto IL_00a9;
		IL_00a9:
		shaderInputPlacingIsland.Update(num2);
	}

	private void HandleBlueprintPaste()
	{
		if (!Player.CurrentMap.InteractionMode.AllowBlueprints(Player))
		{
			DialogStack.ShowUIDialog<HUDDialogSimpleInfo>().InitDialogContents("blueprint-details.dialog-bp-paste-not-unlocked.title".tr(), "blueprint-details.dialog-bp-paste-not-unlocked.description".tr());
			Globals.UISounds.PlayError();
			return;
		}
		string contents = GUIUtility.systemCopyBuffer.Trim();
		if (contents.Length < "SHAPEZ2-".Length)
		{
			Globals.UISounds.PlayError();
			return;
		}
		if (!BlueprintSerializer.TryDeserialize(contents, out var blueprint, out var exception, trySanitize: true))
		{
			Debug.Assert(exception != null);
			DialogStack.ShowUIDialog<HUDDialogSimpleInfo>().InitDialogContents("blueprint-details.dialog-bp-paste-error.title".tr(), "blueprint-details.dialog-bp-paste-error.description".tr(("<error>", exception.tr())));
			Globals.UISounds.PlayError();
			return;
		}
		if (blueprint is IslandBlueprint && !ResearchManager.Progress.IsUnlocked(GameMode.ResearchConfig.IslandManagementUnlock))
		{
			DialogStack.ShowUIDialog<HUDDialogSimpleInfo>().InitDialogContents("blueprint-details.dialog-bp-paste-no-islands-yet.title".tr(), "blueprint-details.dialog-bp-paste-no-islands-yet.description".tr());
			Globals.UISounds.PlayError();
			return;
		}
		if (exception != null)
		{
			DialogStack.ShowUIDialog<HUDDialogSimpleInfo>().InitDialogContents("blueprint-details.dialog-bp-paste-sanitize.title".tr(), "blueprint-details.dialog-bp-paste-sanitize.description".tr(("<error>", exception.tr())));
		}
		ActionSelectBlueprint action = new ActionSelectBlueprint(Player, blueprint);
		if (!ActionManager.TryScheduleAction(action))
		{
			Globals.UISounds.PlayError();
		}
		else
		{
			Globals.UISounds.PlayClick();
		}
	}
}
