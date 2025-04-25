using System;
using Unity.Mathematics;

public class HUDTrainItemsVisualization : HUDVisualization, IDisposable
{
	private readonly DrawManager DrawManager;

	private readonly ResearchManager Research;

	private readonly GameModeHandle GameMode;

	public HUDTrainItemsVisualization(DrawManager drawManager, ResearchManager research, GameModeHandle gameMode)
	{
		DrawManager = drawManager;
		Research = research;
		GameMode = gameMode;
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawTrain = (DrawHooks.DrawTrainDelegate)Delegate.Combine(hooks.OnDrawTrain, new DrawHooks.DrawTrainDelegate(DrawTrain));
	}

	public void Dispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawTrain = (DrawHooks.DrawTrainDelegate)Delegate.Remove(hooks.OnDrawTrain, new DrawHooks.DrawTrainDelegate(DrawTrain));
	}

	public override string GetGlobalIconId()
	{
		return "visualization-train-items";
	}

	public override string GetTitle()
	{
		return "visualizations.train-items.title".tr();
	}

	public override bool IsAvailable()
	{
		return (Player.Viewport.Scope == GameScope.Islands || Player.Viewport.Scope == GameScope.Trains) && Research.Progress.IsUnlocked(GameMode.ResearchConfig.RailsUnlock);
	}

	protected void DrawTrain(FrameDrawOptions options, Train train, float3 pos_W, float angle)
	{
		if (Alpha < 0.001f)
		{
			return;
		}
		BeltItem item = train.ComputeRepresentativeItem();
		if (item != null)
		{
			float scale = 5f + options.Player.Viewport.Zoom / 50f;
			scale *= Alpha;
			scale *= 1f + 0.1f * HUDTheme.PulseAnimation();
			if (item is ShapeCrateItem shapeCrate)
			{
				item = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeCrate.Definition.Hash);
			}
			options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), material: item.GetMaterial(), mesh: item.GetMesh(), transform: FastMatrix.TranslateScale(pos_W + new float3(0f, 5f, 0f), (float3)scale));
		}
	}
}
