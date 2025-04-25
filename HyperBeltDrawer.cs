#define UNITY_ASSERTIONS
using Unity.Mathematics;
using UnityEngine;

public class HyperBeltDrawer : IDrawer<HyperBelt>
{
	private readonly GameMap Map;

	private readonly IIslandMaterializer<HyperBeltNode> Materializer;

	private readonly IPlacementValidator<HyperBelt> HyperBeltPlacementValidator;

	private readonly IPlacementValidator<HyperBeltNode> HyperBeltNodePlacementValidator;

	private static void DrawTunnelConnections(FrameDrawOptions draw, HyperBelt item)
	{
		for (int i = 0; i < item.Nodes.Length - 1; i++)
		{
			HyperBeltNode current = item.Nodes[i];
			if (current.Part == HyperBeltPart.TunnelSender)
			{
				HyperBeltNode receiver = item.Nodes[i + 1];
				if (receiver.Part != HyperBeltPart.Invalid)
				{
					Debug.Assert(receiver.Part == HyperBeltPart.TunnelReceiver);
					RenderPath(draw, current, receiver);
				}
			}
		}
	}

	private static void RenderPath(FrameDrawOptions drawOptions, HyperBeltNode sender, HyperBeltNode receiver)
	{
		uint steps = sender.Position.DistanceManhattan(receiver.Position);
		Grid.Direction dir = Grid.OffsetToDirection((int2)(receiver.Position - sender.Position));
		for (int i = 1; i < steps; i++)
		{
			GlobalChunkCoordinate coordinate_GC = sender.Position + (ChunkDirection)dir * i;
			float3 basePos_W = coordinate_GC.ToCenter_W(-10f);
			if (GeometryUtility.TestPlanesAABB(drawOptions.CameraPlanes, new Bounds(basePos_W, Vector3.one * 20f)))
			{
				TileDirection offset = 3 * TileDirection.South.Rotate(sender.Direction);
				float3 coordinate_W = coordinate_GC.ToCenter_W(-9f) + offset.To_W();
				int size = 20;
				drawOptions.Draw3DPlaneWithMaterial(drawOptions.Theme.BaseResources.UXTunnelsVisualizationConnectorMaterial, Matrix4x4.TRS(coordinate_W, s: new Vector3(size, 1f, size), q: FastMatrix.RotateY(Grid.OppositeDirection(sender.Direction))));
			}
		}
	}

	public HyperBeltDrawer(GameMap map, IIslandMaterializer<HyperBeltNode> materializer, IPlacementValidator<HyperBelt> hyperBeltPlacementValidator, IPlacementValidator<HyperBeltNode> hyperBeltNodePlacementValidator)
	{
		Map = map;
		Materializer = materializer;
		HyperBeltPlacementValidator = hyperBeltPlacementValidator;
		HyperBeltNodePlacementValidator = hyperBeltNodePlacementValidator;
	}

	public void Draw(FrameDrawOptions draw, in HyperBelt data)
	{
		DrawIslands(draw, data);
		DrawTunnelConnections(draw, data);
	}

	private void DrawIslands(FrameDrawOptions draw, HyperBelt item)
	{
		bool canPlaceHyperBelt = HyperBeltPlacementValidator.CanPlace(item);
		for (int i = 0; i < item.Nodes.Length; i++)
		{
			IslandDescriptor island = Materializer.Materialize(item.Nodes[i]);
			VisualTheme.IslandRenderData renderData = new VisualTheme.IslandRenderData(island.FirstChunk_GC, island.Layout, island.LayoutRotation, canPlaceHyperBelt && HyperBeltNodePlacementValidator.CanPlace(item.Nodes[i]));
			draw.Theme.Draw_IslandPreview(draw, Map, renderData);
		}
	}

	void IDrawer<HyperBelt>.Draw(FrameDrawOptions draw, in HyperBelt data)
	{
		Draw(draw, in data);
	}
}
