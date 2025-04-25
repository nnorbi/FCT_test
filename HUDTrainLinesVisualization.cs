using System;
using Unity.Mathematics;
using UnityEngine;

public class HUDTrainLinesVisualization : HUDVisualization, IDisposable
{
	private readonly DrawManager DrawManager;

	private readonly ResearchManager Research;

	private readonly GameModeHandle GameMode;

	public HUDTrainLinesVisualization(DrawManager drawManager, ResearchManager research, GameModeHandle gameMode)
	{
		DrawManager = drawManager;
		Research = research;
		GameMode = gameMode;
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawRail = (DrawHooks.DrawRailDelegate)Delegate.Combine(hooks.OnDrawRail, new DrawHooks.DrawRailDelegate(DrawRail));
		DrawHooks hooks2 = DrawManager.Hooks;
		hooks2.OnDrawTrain = (DrawHooks.DrawTrainDelegate)Delegate.Combine(hooks2.OnDrawTrain, new DrawHooks.DrawTrainDelegate(DrawTrain));
	}

	public void Dispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawRail = (DrawHooks.DrawRailDelegate)Delegate.Remove(hooks.OnDrawRail, new DrawHooks.DrawRailDelegate(DrawRail));
		DrawHooks hooks2 = DrawManager.Hooks;
		hooks2.OnDrawTrain = (DrawHooks.DrawTrainDelegate)Delegate.Remove(hooks2.OnDrawTrain, new DrawHooks.DrawTrainDelegate(DrawTrain));
	}

	public override string GetGlobalIconId()
	{
		return "visualization-train-lines";
	}

	public override string GetTitle()
	{
		return "visualizations.train-lines.title".tr();
	}

	public override bool IsAvailable()
	{
		return (Player.Viewport.Scope == GameScope.Islands || Player.Viewport.Scope == GameScope.Trains) && Research.Progress.IsUnlocked(GameMode.ResearchConfig.RailsUnlock);
	}

	public override bool IsForcedActive()
	{
		return Player.Viewport.Scope == GameScope.Trains;
	}

	protected void DrawRail(FrameDrawOptions options, TrainRailNode rail)
	{
		if (Alpha < 0.001f)
		{
			return;
		}
		foreach (TrainSubPath connection in rail.Connections)
		{
			if (!rail.HasAuthorityOver(connection.To))
			{
				continue;
			}
			foreach (MetaShapeColor color in connection.Colors)
			{
				TrainRailNode.DrawRailOverviewUX(options, rail.Position_TG, connection.To.Position_TG, connection.Descriptor.Type, connection.Descriptor.Direction, color.Color, isPending: false, Alpha);
			}
		}
	}

	protected void DrawTrain(FrameDrawOptions options, Train train, float3 pos_W, float angle)
	{
		if (!(Alpha < 0.001f))
		{
			float scale = 10f + options.Viewport.Zoom / 80f;
			scale *= Alpha;
			options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXTrainLinesVisualizationTrainMaterial, Matrix4x4.TRS(pos_W + new float3(0f, 2f, 0f), Quaternion.Euler(0f, angle, 0f), Vector3.one * scale), MaterialPropertyHelpers.CreateBaseColorBlock(train.Color.Color));
		}
	}
}
