using System;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDBuildingToolbar : HUDBaseToolbar, IRunnableView, IView
{
	[SerializeField]
	private PrefabViewReference<HUDBuildingToolbarSlot> UIToolbarSlotPrefab;

	[SerializeField]
	private EditorDict<BuildingCategory, Sprite> UICategorySprites;

	protected BuildingCategory CurrentCategory;

	private GameModeHandle GameMode;

	private ITutorialStateReadAccess TutorialState;

	public void Run()
	{
		RenderCategory(BuildingCategory.Shapes, force: true);
	}

	[Construct]
	private void Construct(GameModeHandle mode, ITutorialStateReadAccess tutorialState)
	{
		GameMode = mode;
		TutorialState = tutorialState;
		Events.HUDInitialized.AddListener(InitMainButtons);
		Player.SelectedBuildingVariant.Changed.AddListener(OnPlayerSelectedBuildingVariantChanged);
	}

	protected void OnPlayerSelectedBuildingVariantChanged(MetaBuildingVariant variant)
	{
		if (variant != null && !variant.Building.Categories.Contains(CurrentCategory))
		{
			RenderCategory(variant.Building.Categories[0]);
		}
	}

	protected override void OnDispose()
	{
		Events.HUDInitialized.RemoveListener(InitMainButtons);
		Player.SelectedBuildingVariant.Changed.RemoveListener(OnPlayerSelectedBuildingVariantChanged);
		base.OnDispose();
	}

	protected bool IsAnyBuildingOfCategoryUnlocked(BuildingCategory category)
	{
		List<MetaBuilding> buildings = Singleton<GameCore>.G.Mode.Buildings;
		for (int i = 0; i < buildings.Count; i++)
		{
			MetaBuilding building = buildings[i];
			if (building.Categories.Contains(category) && category == building.Categories[0] && IsBuildingUnlocked(building))
			{
				return true;
			}
		}
		return false;
	}

	protected bool IsBuildingUnlocked(MetaBuilding building)
	{
		return building.Variants.Any(Singleton<GameCore>.G.Research.Progress.IsUnlocked);
	}

	protected void RenderCategory(BuildingCategory category, bool force = false)
	{
		if (!force && category == CurrentCategory)
		{
			return;
		}
		CurrentCategory = category;
		RemoveAllSlots();
		int hotkeyIndex = 0;
		foreach (MetaBuilding building in Singleton<GameCore>.G.Mode.Buildings)
		{
			if (building.Categories.IndexOf(CurrentCategory) >= 0 && building.Variants.Count != 0)
			{
				HUDBuildingToolbarSlot slot = AddSlot(UIToolbarSlotPrefab);
				slot.Hotkey = "toolbar.select-slot-" + hotkeyIndex;
				slot.Building = building;
				hotkeyIndex++;
			}
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		SetVisible(Player.Viewport.Scope == GameScope.Buildings && Player.CurrentMap.InteractionMode.AllowBuildingPlacement(Player) && Player.CurrentBlueprint == null);
		base.OnGameUpdate(context, drawOptions);
	}

	protected void InitMainButtons()
	{
		int slotIndex = 0;
		BuildingCategory[] array = (BuildingCategory[])Enum.GetValues(typeof(BuildingCategory));
		foreach (BuildingCategory category in array)
		{
			RegisterMainButton(category, slotIndex);
			slotIndex++;
		}
	}

	protected void RegisterMainButton(BuildingCategory category, int slotIndex)
	{
		Events.RegisterMainButton.Invoke(new HUDMainButtonConfig
		{
			Icon = UICategorySprites.Get(category),
			Part = this,
			Location = HUDMainButtons.ButtonLocation.Middle,
			TooltipHeaderId = "building-toolbar.category-" + category.ToString() + ".title",
			KeybindingId = "toolbar.select-toolbar-" + slotIndex,
			IsEnabled = () => IsAnyBuildingOfCategoryUnlocked(category),
			IsActive = () => CurrentCategory == category,
			IsVisible = () => Player.Viewport.Scope == GameScope.Buildings,
			HasBadge = () => AnyBuildingOfCategoryNewlyUnlocked(category),
			OnActivate = delegate
			{
				OnMainButtonActivated(category);
			}
		});
	}

	protected bool AnyBuildingOfCategoryNewlyUnlocked(BuildingCategory category)
	{
		foreach (MetaBuilding building in GameMode.Buildings)
		{
			if (!building.Categories.Contains(category))
			{
				continue;
			}
			foreach (MetaBuildingVariant variant in building.Variants)
			{
				if (!Player.CurrentMap.InteractionMode.AllowBuildingVariant(Player, variant) || !variant.ShowInToolbar || TutorialState.HasInteractedWithBuilding(variant))
				{
					continue;
				}
				return true;
			}
		}
		return false;
	}

	protected void OnMainButtonActivated(BuildingCategory category)
	{
		if (CurrentCategory != category)
		{
			bool clear = category != CurrentCategory;
			RenderCategory(category);
			if (clear)
			{
				Player.SelectedBuildingVariant.Value = null;
			}
		}
	}
}
