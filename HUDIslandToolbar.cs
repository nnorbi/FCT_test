#define UNITY_ASSERTIONS
using System;
using System.Linq;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public class HUDIslandToolbar : HUDBaseToolbar
{
	[SerializeField]
	private PrefabViewReference<HUDIslandToolbarSlot> ToolbarSlotPrefab;

	[SerializeField]
	private EditorDict<IslandLayoutCategory, Sprite> UICategorySprites;

	protected IslandLayoutCategory CurrentCategory;

	private ResearchManager ResearchManager;

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
		Player.SelectedIslandLayout.Changed.AddListener(OnPlayerSelectedIslandLayoutChanged);
		ResearchManager.Progress.OnChanged.AddListener(OnResearchChanged);
		RenderCategory(IslandLayoutCategory.RegularPlatform);
		Events.HUDInitialized.AddListener(InitMainButtons);
	}

	protected override void OnDispose()
	{
		Player.SelectedIslandLayout.Changed.RemoveListener(OnPlayerSelectedIslandLayoutChanged);
		ResearchManager.Progress.OnChanged.RemoveListener(OnResearchChanged);
		Events.HUDInitialized.RemoveListener(InitMainButtons);
		base.OnDispose();
	}

	protected void OnResearchChanged()
	{
		RenderCategory(CurrentCategory, force: true);
	}

	protected void OnPlayerSelectedIslandLayoutChanged(MetaIslandLayout layout)
	{
		if (!(layout == null) && layout.Categories.Length != 0 && !layout.Categories.Contains(CurrentCategory))
		{
			IslandLayoutCategory category = layout.Categories[0];
			RenderCategory(category);
		}
	}

	protected bool IsAnyLayoutOfCategoryUnlocked(IslandLayoutCategory category)
	{
		foreach (MetaIslandLayout layout in Singleton<GameCore>.G.Mode.IslandLayouts)
		{
			if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(layout) || !layout.Categories.Contains(category))
			{
				continue;
			}
			return true;
		}
		return false;
	}

	protected void RenderCategory(IslandLayoutCategory category, bool force = false)
	{
		if (!force && category == CurrentCategory)
		{
			return;
		}
		CurrentCategory = category;
		RemoveAllSlots();
		int hotkeyIndex = 0;
		foreach (MetaIslandLayout layout in Singleton<GameCore>.G.Mode.IslandLayouts)
		{
			if (layout.PlayerBuildable)
			{
				Debug.Assert(layout.Categories != null, "Layout " + layout.name + " has no categories");
				if (layout.Categories.Length != 0 && layout.Categories.Contains(CurrentCategory))
				{
					HUDIslandToolbarSlot slot = AddSlot(ToolbarSlotPrefab);
					slot.Hotkey = "toolbar.select-slot-" + hotkeyIndex;
					slot.Layout = layout;
					hotkeyIndex++;
				}
			}
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		SetVisible(Player.Viewport.Scope == GameScope.Islands && Player.CurrentMap.InteractionMode.AllowIslandManagement(Player) && Player.CurrentBlueprint == null);
		base.OnGameUpdate(context, drawOptions);
	}

	protected void InitMainButtons()
	{
		int slotIndex = 0;
		IslandLayoutCategory[] categories = (IslandLayoutCategory[])Enum.GetValues(typeof(IslandLayoutCategory));
		IslandLayoutCategory[] array = categories;
		foreach (IslandLayoutCategory category in array)
		{
			RegisterMainButton(category, slotIndex);
			slotIndex++;
		}
	}

	protected void RegisterMainButton(IslandLayoutCategory category, int slotIndex)
	{
		Events.RegisterMainButton.Invoke(new HUDMainButtonConfig
		{
			Icon = UICategorySprites.Get(category),
			Part = this,
			Location = HUDMainButtons.ButtonLocation.Middle,
			TooltipHeaderId = "island-toolbar.category-" + category.ToString() + ".title",
			KeybindingId = "toolbar.select-toolbar-" + slotIndex,
			IsEnabled = () => IsAnyLayoutOfCategoryUnlocked(category),
			IsActive = () => Player.Viewport.Scope == GameScope.Islands && Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.IslandManagementUnlock) && CurrentCategory == category,
			IsVisible = () => Player.Viewport.Scope == GameScope.Islands,
			HasBadge = () => false,
			OnActivate = delegate
			{
				OnMainButtonActivated(category);
			}
		});
	}

	protected void OnMainButtonActivated(IslandLayoutCategory category)
	{
		if (CurrentCategory != category)
		{
			bool clear = category != CurrentCategory;
			RenderCategory(category);
			if (clear)
			{
				Player.SelectedIslandLayout.Value = null;
			}
		}
	}
}
