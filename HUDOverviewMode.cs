using System;
using Core.Dependency;
using DG.Tweening;
using UnityEngine;

public class HUDOverviewMode : HUDPart
{
	public const float LAYER_HEIGHT_PLANE = -3f;

	public const float LAYER_HEIGHT_EXPLORED_AREA = -2.5f;

	public const float LAYER_HEIGHT_FLUIDS = -2f;

	public const float LAYER_HEIGHT_SHAPES = -2f;

	public const float LAYER_HEIGHT_TUNNELS = -1f;

	public const float LAYER_HEIGHT_RAILS = 0f;

	public const float LAYER_HEIGHT_TRAINS = 1f;

	public const float LAYER_HEIGHT_ISLANDS = 3.5f;

	private float Alpha = 0f;

	private bool Active = false;

	private Sequence ActiveSequence;

	private HUDOverviewModeBackgroundPlaneRenderer BackgroundPlaneRenderer;

	private HUDOverviewModeFluidsRenderer FluidsRenderer;

	private HUDOverviewModeShapesRenderer ShapesRenderer;

	private HUDOverviewModeIslandsRenderer IslandsRenderer;

	private HUDOverviewModeRailsRenderer RailsRenderer;

	private HUDOverviewModeTrainsRenderer TrainsRenderer;

	private HUDOverviewModeTunnelsRenderer TunnelsRenderer;

	private HUDOverviewModeExploredAreaRenderer ExploredAreaRenderer;

	private DrawManager DrawManager;

	[Construct]
	private void Construct(DrawManager drawManager)
	{
		DrawManager = drawManager;
		BackgroundPlaneRenderer = new HUDOverviewModeBackgroundPlaneRenderer();
		FluidsRenderer = new HUDOverviewModeFluidsRenderer();
		ShapesRenderer = new HUDOverviewModeShapesRenderer();
		IslandsRenderer = new HUDOverviewModeIslandsRenderer();
		RailsRenderer = new HUDOverviewModeRailsRenderer();
		TrainsRenderer = new HUDOverviewModeTrainsRenderer();
		TunnelsRenderer = new HUDOverviewModeTunnelsRenderer();
		ExploredAreaRenderer = new HUDOverviewModeExploredAreaRenderer();
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawSuperChunk = (DrawHooks.DrawSuperChunkDelegate)Delegate.Combine(hooks.OnDrawSuperChunk, new DrawHooks.DrawSuperChunkDelegate(DrawSuperChunk));
	}

	protected override void OnDispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawSuperChunk = (DrawHooks.DrawSuperChunkDelegate)Delegate.Remove(hooks.OnDrawSuperChunk, new DrawHooks.DrawSuperChunkDelegate(DrawSuperChunk));
	}

	private void SetActive(bool active)
	{
		if (active != Active)
		{
			bool fadingOut = !active;
			ActiveSequence?.Kill();
			ActiveSequence = DOTween.Sequence();
			ActiveSequence.Append(DOTween.To(() => Alpha, delegate(float v)
			{
				Alpha = v;
			}, active ? 1 : 0, 0.25f).SetEase(fadingOut ? Ease.OutExpo : Ease.OutExpo));
			Active = active;
		}
	}

	private void DrawSuperChunk(FrameDrawOptions options, MapSuperChunk chunk)
	{
		if (!(Alpha < 0.001f))
		{
			FluidsRenderer.DrawSuperChunk(options, chunk, Alpha);
			ShapesRenderer.DrawSuperChunk(options, chunk, Alpha);
			IslandsRenderer.DrawSuperChunk(options, chunk, Alpha);
			TunnelsRenderer.DrawSuperChunk(options, chunk, Alpha);
			ExploredAreaRenderer.DrawSuperChunk(options, chunk, Alpha);
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		bool active = Player.Viewport.Scope == GameScope.Overview;
		SetActive(active);
		float alpha = Alpha;
		float zoom = Player.Viewport.Zoom;
		if ((zoom > 8000f || zoom < 200f) && ActiveSequence != null)
		{
			Debug.LogWarning("Performing fast switch for overview mode at " + zoom + " to avoid performance break");
			ActiveSequence?.Complete();
			ActiveSequence = null;
		}
		if (!(alpha < 0.001f))
		{
			if (alpha > 0.999f)
			{
				context.ConsumeToken("visualtheme:background");
				context.ConsumeToken("visualtheme:fluid_resources");
				context.ConsumeToken("visualtheme:shape_resources");
				context.ConsumeToken("visualtheme:islands");
				context.ConsumeToken("trains::render-rails");
				context.ConsumeToken("trains::render-trains");
			}
			BackgroundPlaneRenderer.Draw(drawOptions, alpha);
			RailsRenderer.Draw(drawOptions, alpha);
			TrainsRenderer.Draw(drawOptions, alpha);
		}
	}
}
