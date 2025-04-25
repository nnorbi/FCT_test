using System;
using Unity.Mathematics;
using UnityEngine;

public class HUDTunnelsVisualization : HUDVisualization, IDisposable
{
	private readonly DrawManager DrawManager;

	public HUDTunnelsVisualization(DrawManager drawManager)
	{
		DrawManager = drawManager;
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawIslandAlwaysNeedsManualCulling = (DrawHooks.DrawIslandDelegate)Delegate.Combine(hooks.OnDrawIslandAlwaysNeedsManualCulling, new DrawHooks.DrawIslandDelegate(DrawTunnel));
	}

	public void Dispose()
	{
		DrawHooks hooks = DrawManager.Hooks;
		hooks.OnDrawIslandAlwaysNeedsManualCulling = (DrawHooks.DrawIslandDelegate)Delegate.Remove(hooks.OnDrawIslandAlwaysNeedsManualCulling, new DrawHooks.DrawIslandDelegate(DrawTunnel));
	}

	public override string GetGlobalIconId()
	{
		return "visualization-tunnels";
	}

	public override string GetTitle()
	{
		return "visualizations.tunnels.title".tr();
	}

	public override bool IsAvailable()
	{
		return Player.Viewport.Scope == GameScope.Islands;
	}

	protected void DrawTunnel(FrameDrawOptions options, Island island)
	{
		if (Alpha < 0.001f || !(island is TunnelEntranceIsland { CachedExit: { } receiver } entrance))
		{
			return;
		}
		bool goViaX = entrance.Origin_GC.y == receiver.Origin_GC.y;
		int start = (goViaX ? entrance.Origin_GC.x : entrance.Origin_GC.y);
		int end = (goViaX ? receiver.Origin_GC.x : receiver.Origin_GC.y);
		if (start == end)
		{
			GlobalChunkCoordinate origin_GC = entrance.Origin_GC;
			string text = origin_GC.ToString();
			origin_GC = receiver.Origin_GC;
			throw new Exception("invalid tunnel connection: " + text + " vs " + origin_GC.ToString());
		}
		TunnelEntranceEntity building = (TunnelEntranceEntity)entrance.Buildings.Buildings[0];
		BeltItem representativeItem = null;
		foreach (TunnelLane lane2 in building.Lanes)
		{
			BeltItem laneItem = lane2.GetRepresentativeItem();
			if (laneItem != null)
			{
				representativeItem = laneItem;
				break;
			}
		}
		if (representativeItem != null)
		{
			float3 entranceCenter_W = entrance.Origin_GC.ToCenter_W(3f);
			float3 receiverCenter_W = entrance.Origin_GC.ToCenter_W(3f);
			float3 entranceToReceiver_W = receiverCenter_W - entranceCenter_W;
			float3 center_W = entranceCenter_W + 0.5f * entranceToReceiver_W;
			float scale = 15f + options.Player.Viewport.Zoom / 30f;
			scale *= Alpha;
			HUDIslandShapeTransferVisualization.DrawTransferredItem(options, representativeItem, center_W, scale);
		}
		int steps = math.abs(end - start);
		int delta = (int)math.sign(end - start);
		for (int i = 1; i < steps; i++)
		{
			float3 pos_W = (entrance.Origin_GC + (goViaX ? new ChunkDirection(i * delta, 0) : new ChunkDirection(0, i * delta))).ToCenter_W(-9f);
			if (GeometryUtility.TestPlanesAABB(options.CameraPlanes, new Bounds(pos_W, Vector3.one * 20f)))
			{
				options.Draw3DPlaneWithMaterial(options.Theme.BaseResources.UXTunnelsVisualizationConnectorMaterial, Matrix4x4.TRS(pos_W, s: new Vector3(20f, 1f, 20f * Alpha), q: FastMatrix.RotateY(Grid.OppositeDirection(entrance.Metadata.LayoutRotation))));
			}
		}
	}
}
