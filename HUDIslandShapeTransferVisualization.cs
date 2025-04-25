using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class HUDIslandShapeTransferVisualization : HUDVisualization, IDisposable
{
	private static int INSTANCING_ID_SENDER = Shader.PropertyToID("island-shape-transfer-visualization:sender");

	private static int INSTANCING_ID_RECEIVER = Shader.PropertyToID("island-shape-transfer-visualization:receiver");

	private readonly DrawManager DrawManager;

	public static void DrawTransferredItem(FrameDrawOptions options, BeltItem item, float3 pos_W, float scale)
	{
		options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), item.GetMesh(), item.GetMaterial(), FastMatrix.TranslateScale(in pos_W, (float3)scale));
		options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXIslandShapeTransferVisualizationShapeUnderlayMaterial, FastMatrix.TranslateScale(in pos_W, (float3)(scale * 1.3f)));
	}

	public HUDIslandShapeTransferVisualization(DrawManager drawManager)
	{
		DrawManager = drawManager;
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawIslandNotch = (DrawHooks.DrawIslandNotchDelegate)Delegate.Combine(hooks.OnDrawIslandNotch, new DrawHooks.DrawIslandNotchDelegate(DrawNotch));
	}

	public void Dispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawIslandNotch = (DrawHooks.DrawIslandNotchDelegate)Delegate.Remove(hooks.OnDrawIslandNotch, new DrawHooks.DrawIslandNotchDelegate(DrawNotch));
	}

	public override bool IsAvailable()
	{
		return Player.Viewport.Scope == GameScope.Islands;
	}

	public override string GetGlobalIconId()
	{
		return "visualization-island-shape-transfer";
	}

	public override string GetTitle()
	{
		return "visualizations.island-shape-transfer.title".tr();
	}

	protected void DrawNotch(FrameDrawOptions options, IslandChunkNotch notch)
	{
		if (Alpha < 0.001f || !notch.BuildingsOnNotch.Any())
		{
			return;
		}
		float scale = 3f + options.Player.Viewport.Zoom / 150f;
		scale *= Alpha;
		scale *= 1f + 0.1f * HUDTheme.PulseAnimation();
		float offset = 0f + HUDTheme.PulseAnimation();
		float3 chunkCenter_W = notch.Chunk.Coordinate_GC.ToCenter_W();
		Grid.Direction notchDirection = notch.Direction;
		WorldDirection notchDirection_W = notchDirection;
		if (notch.ContainsBuildingWithMode(IslandNotchBuildingMode.SendingShapes))
		{
			float3 indicatorPos_W = chunkCenter_W + (4.5f + offset) * notchDirection_W;
			options.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID_SENDER, options.Theme.BaseResources.UXIslandShapeTransferVisualizationSenderMaterial, Matrix4x4.TRS(q: FastMatrix.RotateY(Grid.OppositeDirection(notchDirection)), s: new Vector3(scale, 1f, scale), pos: indicatorPos_W + 3.5f * WorldDirection.Up));
			BeltItem item = notch.ComputeRepresentativeShapeTransferItem();
			if (item != null)
			{
				float3 shapePos_W = chunkCenter_W + (9f + offset) * notchDirection_W;
				DrawTransferredItem(options, item, shapePos_W + 3.5f * WorldDirection.Up, scale * 3f);
			}
		}
		if (notch.ContainsBuildingWithMode(IslandNotchBuildingMode.ReceivingShapes))
		{
			options.Draw3DPlaneWithMaterialInstanced(INSTANCING_ID_RECEIVER, options.Theme.BaseResources.UXIslandShapeTransferVisualizationReceiverMaterial, Matrix4x4.TRS(q: FastMatrix.RotateY(Grid.OppositeDirection(notchDirection)), s: new Vector3(scale, 1f, scale), pos: chunkCenter_W + (6.5f - offset) * notchDirection_W + 3f * WorldDirection.Up));
		}
	}
}
