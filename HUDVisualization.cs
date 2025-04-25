using DG.Tweening;

public abstract class HUDVisualization
{
	protected HUDEvents Events;

	protected Player Player;

	protected Sequence ActiveSequence;

	protected float Alpha = 0f;

	public bool UserEnabled { get; private set; } = false;

	public bool Active { get; private set; } = false;

	public void LinkWithHUD(HUDEvents events, Player player)
	{
		Events = events;
		Player = player;
	}

	public abstract string GetGlobalIconId();

	public abstract string GetTitle();

	protected virtual float GetAnimationDuration(bool fadingOut)
	{
		return 0.4f;
	}

	protected virtual Ease GetAnimationEasing(bool fadingOut)
	{
		return Ease.OutExpo;
	}

	public void ToggleEnabled()
	{
		UserEnabled = !UserEnabled;
	}

	public void SetEnabled(bool enabled)
	{
		UserEnabled = enabled;
	}

	public virtual bool IsAvailable()
	{
		return Player.Viewport.Scope == GameScope.Islands;
	}

	public virtual bool IsForcedActive()
	{
		return false;
	}

	public void SetActive(bool active)
	{
		if (active != Active)
		{
			bool fadingOut = !active;
			ActiveSequence?.Kill();
			ActiveSequence = DOTween.Sequence();
			ActiveSequence.Append(DOTween.To(() => Alpha, delegate(float v)
			{
				Alpha = v;
			}, active ? 1 : 0, GetAnimationDuration(fadingOut)).SetEase(GetAnimationEasing(fadingOut)));
			Active = active;
		}
	}

	public virtual void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions options)
	{
	}
}
