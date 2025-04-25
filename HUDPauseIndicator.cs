using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class HUDPauseIndicator : HUDPart
{
	public static string TOKEN_PAUSE_INDICATOR_VISIBLE = "HUDPauseIndicator$pause_indicator_visible";

	[SerializeField]
	private CanvasGroup UIMainGroup;

	protected bool Visible = false;

	public override bool NeedsGraphicsRaycaster => false;

	[Construct]
	private void Construct()
	{
		UIMainGroup.alpha = 0f;
		base.gameObject.SetActiveSelfExt(active: false);
	}

	protected override void OnDispose()
	{
	}

	protected void SetVisible(bool visible)
	{
		if (Visible != visible)
		{
			Visible = visible;
			DOTween.Kill(UIMainGroup);
			base.gameObject.SetActiveSelfExt(active: true);
			UIMainGroup.DOFade(visible ? 1 : 0, 0.25f).OnComplete(delegate
			{
			}).OnComplete(delegate
			{
				base.gameObject.SetActiveSelfExt(visible);
			});
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		SetVisible(context.ConsumeToken(TOKEN_PAUSE_INDICATOR_VISIBLE) && Singleton<GameCore>.G.SimulationSpeed.Paused);
	}
}
