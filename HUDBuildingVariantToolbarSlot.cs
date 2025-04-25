using Core.Dependency;

public class HUDBuildingVariantToolbarSlot : HUDBaseToolbarSlot
{
	private MetaBuildingVariant _Variant;

	private Player Player;

	private ResearchManager ResearchManager;

	private ITutorialStateReadAccess TutorialState;

	private ITutorialHighlightProvider TutorialHighlightProvider;

	public MetaBuildingVariant Variant
	{
		get
		{
			return _Variant;
		}
		set
		{
			if (!(value == _Variant))
			{
				_Variant = value;
				base.TooltipText = _Variant.Title;
				SetIcon(_Variant.Icon);
				UpdateState();
				OnHighlightChanged();
			}
		}
	}

	protected bool Unlocked => ResearchManager.Progress.IsUnlocked(Variant);

	[Construct]
	private void Construct(Player player, ResearchManager researchManager, ITutorialStateReadAccess tutorialState, ITutorialHighlightProvider tutorialHighlightProvider)
	{
		Player = player;
		ResearchManager = researchManager;
		TutorialState = tutorialState;
		TutorialHighlightProvider = tutorialHighlightProvider;
		Player.SelectedBuildingVariant.Changed.AddListener(OnPlayerSelectedBuildingVariantChanged);
		TutorialHighlightProvider.HighlightChanged.AddListener(OnHighlightChanged);
		ResearchManager.Progress.OnChanged.AddListener(UpdateState);
		OnHighlightChanged();
	}

	private void OnHighlightChanged()
	{
		base.Highlighted = Variant != null && Player.SelectedBuildingVariant.Value?.Building == Variant.Building && TutorialHighlightProvider.IsBuildingVariantHighlighted(Variant);
	}

	protected override void OnDispose()
	{
		ResearchManager.Progress.OnChanged.RemoveListener(UpdateState);
		TutorialHighlightProvider.HighlightChanged.RemoveListener(OnHighlightChanged);
		Player.SelectedBuildingVariant.Changed.RemoveListener(OnPlayerSelectedBuildingVariantChanged);
		base.OnDispose();
	}

	protected void OnPlayerSelectedBuildingVariantChanged(MetaBuildingVariant variant)
	{
		UpdateState();
		OnHighlightChanged();
	}

	protected override void OnSlotClicked()
	{
		Player.SelectedBuildingVariant.Value = Variant;
		Globals.UISounds.PlayToolbarSelectSlot();
	}

	protected void UpdateState()
	{
		bool active = Player.SelectedBuildingVariant == Variant;
		if (Unlocked)
		{
			SetState((!active) ? SlotState.Normal : SlotState.Selected);
		}
		else
		{
			SetState(SlotState.Locked);
		}
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		base.OnUpdate(context);
		if (base.State != SlotState.Locked)
		{
			base.HasBadge = !TutorialState.HasInteractedWithBuilding(Variant);
		}
		else
		{
			base.HasBadge = false;
		}
	}
}
