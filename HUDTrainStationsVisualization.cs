using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class HUDTrainStationsVisualization : HUDVisualization, IDisposable
{
	private readonly DrawManager DrawManager;

	private readonly ResearchManager Research;

	private readonly GameModeHandle GameMode;

	public HUDTrainStationsVisualization(DrawManager drawManager, ResearchManager research, GameModeHandle gameMode)
	{
		DrawManager = drawManager;
		Research = research;
		GameMode = gameMode;
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawIslandNotch = (DrawHooks.DrawIslandNotchDelegate)Delegate.Combine(hooks.OnDrawIslandNotch, new DrawHooks.DrawIslandNotchDelegate(DrawNotch));
	}

	public void Dispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawIslandNotch = (DrawHooks.DrawIslandNotchDelegate)Delegate.Remove(hooks.OnDrawIslandNotch, new DrawHooks.DrawIslandNotchDelegate(DrawNotch));
	}

	public override string GetGlobalIconId()
	{
		return "visualization-train-stations";
	}

	public override string GetTitle()
	{
		return "visualizations.train-stations.title".tr();
	}

	public override bool IsAvailable()
	{
		return (Player.Viewport.Scope == GameScope.Islands || Player.Viewport.Scope == GameScope.Trains) && Research.Progress.IsUnlocked(GameMode.ResearchConfig.RailsUnlock);
	}

	protected void DrawNotch(FrameDrawOptions options, IslandChunkNotch notch)
	{
		if (!(Alpha < 0.001f) && notch.BuildingsOnNotch.Any())
		{
			float scale = 2f + options.Player.Viewport.Zoom / 90f;
			scale *= Alpha;
			scale *= 1f + 0.1f * HUDTheme.PulseAnimation();
			float offset = 0f + HUDTheme.PulseAnimation();
			float3 chunkCenter_W = notch.Chunk.Coordinate_GC.ToCenter_W(3f);
			Grid.Direction notchDirection = notch.Direction;
			if (notch.ContainsBuildingWithMode(IslandNotchBuildingMode.LoadingTrains))
			{
				options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXTrainStationsVisualizationLoaderMaterial, Matrix4x4.TRS(q: FastMatrix.RotateY(notchDirection), s: new Vector3(scale, 1f, scale), pos: chunkCenter_W + (8f + offset) * (WorldDirection)notchDirection));
			}
			if (notch.ContainsBuildingWithMode(IslandNotchBuildingMode.UnloadingTrains))
			{
				options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXTrainStationsVisualizationUnloaderMaterial, Matrix4x4.TRS(q: FastMatrix.RotateY(notchDirection), s: new Vector3(scale, 1f, scale), pos: chunkCenter_W + (8f - offset) * (WorldDirection)notchDirection));
			}
		}
	}
}
