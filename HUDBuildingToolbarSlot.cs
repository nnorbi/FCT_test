#define UNITY_ASSERTIONS
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Core.Events;
using DG.Tweening;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;

public class HUDBuildingToolbarSlot : HUDBaseToolbarSlot
{
	[SerializeField]
	protected PrefabViewReference<HUDBuildingVariantToolbarSlot> BuildingVariantToolbarSlotPrefab;

	[SerializeField]
	protected PrefabViewReference<HUDToolbarSlotVariantsIndicator> ToolbarSlotVariantsIndicatorPrefab;

	[SerializeField]
	protected RectTransform UIVariantsContainerContents;

	[SerializeField]
	protected RectTransform UIVariantsContainer;

	[SerializeField]
	protected RectTransform UIVariantSelectorParent;

	[SerializeField]
	protected CanvasGroup UIVariantsGroup;

	[SerializeField]
	protected CanvasGroup UIVariantsSelectorGroup;

	private HUDToolbarSlotVariantsIndicator ToolbarSlotVariantsIndicator;

	protected Sequence CurrentVariantsSequence;

	protected List<HUDBuildingVariantToolbarSlot> InstantiatedVariants = new List<HUDBuildingVariantToolbarSlot>();

	private MetaBuilding _Building;

	private Player Player;

	private ResearchManager ResearchManager;

	private IEventSender PassiveEventBus;

	private ITutorialStateReadAccess TutorialState;

	private ITutorialHighlightProvider TutorialHighlightProvider;

	private MetaBuildingVariant LastSelectedVariant;

	protected bool HasVariantsSelector => _Building != null && _Building.Variants.Count((MetaBuildingVariant v) => v.PlayerBuildable && v.ShowInToolbar) > 1;

	public MetaBuilding Building
	{
		get
		{
			return _Building;
		}
		set
		{
			if (!(value == _Building))
			{
				_Building = value;
				if (ToolbarSlotVariantsIndicator != null)
				{
					ReleaseChildView(ToolbarSlotVariantsIndicator);
					ToolbarSlotVariantsIndicator = null;
				}
				if (HasVariantsSelector)
				{
					ToolbarSlotVariantsIndicator = RequestChildView(ToolbarSlotVariantsIndicatorPrefab).PlaceAt(UIVariantSelectorParent);
					ToolbarSlotVariantsIndicator.Building = value;
				}
				base.TooltipText = _Building.Title;
				SetIcon(_Building.Icon);
				ClearVariants();
				SetupVariants();
				UpdateState();
				OnHighlightChanged();
			}
		}
	}

	protected bool Unlocked => Building != null && Building.Variants.Any((MetaBuildingVariant variant) => ResearchManager.Progress.IsUnlocked(variant));

	[Construct]
	private void Construct(Player player, ResearchManager researchManager, IEventSender passiveEventBus, ITutorialStateReadAccess tutorialState, ITutorialHighlightProvider tutorialHighlightProvider)
	{
		Player = player;
		ResearchManager = researchManager;
		PassiveEventBus = passiveEventBus;
		TutorialState = tutorialState;
		TutorialHighlightProvider = tutorialHighlightProvider;
		Player.SelectedBuildingVariant.Changed.AddListener(OnPlayerSelectedBuildingVariantChanged);
		ResearchManager.Progress.OnChanged.AddListener(UpdateState);
		TutorialHighlightProvider.HighlightChanged.AddListener(OnHighlightChanged);
		UIVariantsGroup.alpha = 0f;
		UIVariantsSelectorGroup.alpha = 1f;
		UIVariantsContainer.localScale = new Vector3(0f, 0f, 0f);
		UIVariantsContainer.gameObject.SetActiveSelfExt(active: false);
		OnHighlightChanged();
	}

	private void OnHighlightChanged()
	{
		base.Highlighted = Building != null && (Player.SelectedBuildingVariant.Value?.Building != Building || !HasVariantsSelector) && Building.Variants.Any((MetaBuildingVariant variant) => variant.ShowInToolbar && TutorialHighlightProvider.IsBuildingVariantHighlighted(variant) && Player.CurrentMap.InteractionMode.AllowBuildingVariant(Player, variant));
	}

	protected override void OnDispose()
	{
		ResearchManager.Progress.OnChanged.RemoveListener(UpdateState);
		TutorialHighlightProvider.HighlightChanged.RemoveListener(OnHighlightChanged);
		Player.SelectedBuildingVariant.Changed.RemoveListener(OnPlayerSelectedBuildingVariantChanged);
		CurrentVariantsSequence?.Kill();
		CurrentVariantsSequence = null;
		ClearVariants();
		base.OnDispose();
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		base.OnUpdate(context);
		if (base.State == SlotState.Selected && HasVariantsSelector)
		{
			int variantOffset = 0;
			if (context.ConsumeWasActivated("toolbar.next-variant"))
			{
				variantOffset++;
			}
			if (context.ConsumeWasActivated("toolbar.previous-variant"))
			{
				variantOffset--;
			}
			if (variantOffset != 0)
			{
				CycleVariants(variantOffset);
			}
		}
		if (base.State == SlotState.Locked)
		{
			base.HasBadge = false;
			return;
		}
		base.HasBadge = Building.Variants.Any((MetaBuildingVariant v) => Player.CurrentMap.InteractionMode.AllowBuildingVariant(Player, v) && v.ShowInToolbar && !TutorialState.HasInteractedWithBuilding(v));
	}

	protected void CycleVariants(int shift)
	{
		if (Building == null)
		{
			return;
		}
		List<MetaBuildingVariant> variants = Building.Variants;
		int currentIndex = variants.IndexOf(Player.SelectedBuildingVariant.Value);
		Debug.Assert(currentIndex >= 0, "current index < 0");
		int attempts = variants.Count * math.abs(shift) + 1;
		for (int i = 1; i < attempts; i++)
		{
			MetaBuildingVariant variant = variants[FastMath.SafeMod(currentIndex + i * shift, variants.Count)];
			if (variant.ShowInToolbar && Player.CurrentMap.InteractionMode.AllowBuildingVariant(Player, variant))
			{
				PassiveEventBus.Emit(new PlayerCycledBuildingToolbarSlotVariantsEvent(Player));
				Globals.UISounds.PlayToolbarSelectSlot();
				Player.SelectedBuildingVariant.Value = variant;
				break;
			}
		}
	}

	protected void SetupVariants()
	{
		if (!HasVariantsSelector)
		{
			return;
		}
		foreach (MetaBuildingVariant variant in Building.Variants)
		{
			if (variant.ShowInToolbar && variant.PlayerBuildable)
			{
				HUDBuildingVariantToolbarSlot instance = RequestChildView(BuildingVariantToolbarSlotPrefab).PlaceAt(UIVariantsContainerContents);
				instance.Variant = variant;
				InstantiatedVariants.Add(instance);
			}
		}
	}

	protected void ClearVariants()
	{
		foreach (HUDBuildingVariantToolbarSlot instance in InstantiatedVariants)
		{
			ReleaseChildView(instance);
		}
		InstantiatedVariants.Clear();
	}

	protected void OnPlayerSelectedBuildingVariantChanged(MetaBuildingVariant variant)
	{
		if (variant != null && variant.Building == Building)
		{
			LastSelectedVariant = variant;
		}
		UpdateState();
		OnHighlightChanged();
	}

	protected override void OnStateChanged(SlotState state)
	{
		base.OnStateChanged(state);
		if (!HasVariantsSelector)
		{
			return;
		}
		CurrentVariantsSequence?.Kill(complete: true);
		CurrentVariantsSequence = DOTween.Sequence();
		UIVariantsContainer.gameObject.SetActiveSelfExt(active: true);
		bool active = state == SlotState.Selected;
		float targetScale = (active ? 1f : 0f);
		CurrentVariantsSequence.Append(UIVariantsContainer.DOScaleX(targetScale, active ? 0.3f : 0.4f).SetEase(active ? Ease.OutExpo : Ease.OutExpo));
		CurrentVariantsSequence.Join(UIVariantsContainer.DOScaleY(targetScale, active ? 0.2f : 0.22f).SetEase((!active) ? Ease.Linear : Ease.OutExpo));
		CurrentVariantsSequence.Join(UIVariantsSelectorGroup.DOFade(active ? 0f : 1f, 0.12f));
		CurrentVariantsSequence.Join(UIVariantsGroup.DOFade(active ? 1f : 0f, 0.2f));
		CurrentVariantsSequence.OnComplete(delegate
		{
			if (!active)
			{
				UIVariantsContainer.gameObject.SetActiveSelfExt(active: false);
			}
		});
	}

	protected override void OnSlotClicked()
	{
		if (base.State == SlotState.Selected && Player.SelectedBuildingVariant.Value?.Building == Building)
		{
			Player.SelectedBuildingVariant.Value = null;
			return;
		}
		MetaBuildingVariant variant = Building.Variants[0];
		if (LastSelectedVariant != null && Building.SaveSelectedVariant && ResearchManager.Progress.IsUnlocked(LastSelectedVariant))
		{
			variant = LastSelectedVariant;
		}
		if (ResearchManager.Progress.IsUnlocked(variant))
		{
			Player.SelectedBuildingVariant.Value = variant;
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	protected void UpdateState()
	{
		bool isActive = Player.SelectedBuildingVariant.Value?.Building == Building;
		if (Unlocked)
		{
			SlotState newState = ((!isActive) ? SlotState.Normal : SlotState.Selected);
			if (base.State == SlotState.Normal && newState == SlotState.Selected)
			{
				Globals.UISounds.PlayToolbarSelectSlot();
			}
			else if (base.State == SlotState.Selected && newState == SlotState.Normal && Player.SelectedBuildingVariant == null)
			{
				Globals.UISounds.PlayToolbarClearSlot();
			}
			SetState(newState);
		}
		else
		{
			SetState(SlotState.Locked);
		}
	}
}
