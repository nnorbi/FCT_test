using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDMainButtonToolbarSlot : HUDBaseToolbarSlot
{
	[SerializeField]
	private TMP_Text UILabelText;

	private Player Player;

	private HUDMainButtonConfig Config;

	private ITutorialHighlightProvider TutorialHighlightProvider;

	[Construct]
	private void Construct(Player player, ITutorialHighlightProvider tutorialHighlightProvider)
	{
		Player = player;
		TutorialHighlightProvider = tutorialHighlightProvider;
		TutorialHighlightProvider.HighlightChanged.AddListener(OnHighlightChanged);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		TutorialHighlightProvider.HighlightChanged.RemoveListener(OnHighlightChanged);
	}

	private void OnHighlightChanged()
	{
		base.Highlighted = !string.IsNullOrEmpty(Config.KeybindingId) && TutorialHighlightProvider.IsKeybindingHighlighted(Config.KeybindingId);
	}

	public void SetConfig(HUDMainButtonConfig config)
	{
		if (Config != config)
		{
			Config = config;
			SetIcon(config.Icon);
			base.Hotkey = config.KeybindingId;
			base.ListenToKeybinding = config.ListenToKeybinding;
			UILabelText.text = config.TooltipHeaderId.tr();
		}
	}

	protected override void OnSlotClicked()
	{
		Config?.OnActivate?.Invoke();
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (Config == null)
		{
			base.gameObject.SetActiveSelfExt(active: false);
			return;
		}
		if (!Config.IsVisible())
		{
			base.gameObject.SetActiveSelfExt(active: false);
			return;
		}
		base.gameObject.SetActiveSelfExt(active: true);
		if (!Config.IsEnabled())
		{
			SetState(SlotState.Locked);
			base.HasBadge = false;
			return;
		}
		base.HasBadge = Config.HasBadge();
		if (Config.IsActive())
		{
			SetState(SlotState.Selected);
		}
		else
		{
			SetState(SlotState.Normal);
		}
		base.OnUpdate(context);
	}
}
