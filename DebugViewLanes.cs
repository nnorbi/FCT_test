using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class DebugViewLanes : IDebugView
{
	protected class LaneVisualizer : IBeltLaneTraverser
	{
		protected MapEntity Entity;

		protected CommandBuilder Builder;

		public LaneVisualizer(MapEntity entity, CommandBuilder builder)
		{
			Entity = entity;
			Builder = builder;
		}

		public void Traverse(BeltLane lane)
		{
			VisualizeLane(Builder, Entity, lane);
		}
	}

	public const string ID = "lanes";

	public string Name => "Lanes";

	protected static void VisualizeLane(CommandBuilder draw, MapEntity entity, BeltLane lane, float offsetHeight = 0f)
	{
		BeltLaneDefinition definition = lane.Definition;
		float3 start_L = definition.ItemStartPos_L;
		float3 end_L = definition.ItemEndPos_L;
		float3 start_I = entity.I_From_L(in start_L);
		float3 end_I = entity.I_From_L(in end_L);
		float3 start_W = entity.Island.W_From_I(start_I + new float3(0f, 0f, offsetHeight));
		float3 end_W = entity.Island.W_From_I(end_I + new float3(0f, 0f, offsetHeight));
		float3 offset = new float3(0f, 0.5f, 0f);
		draw.Arrow(start_W + offset, end_W + offset, lane.HasItem ? new Color(0f, 1f, 0f) : new Color(0f, 0f, 1f));
		float3 pos_L = lane.Definition.GetPosFromTicks_L(lane.Progress_T);
		if (lane.MaxStep_S > 0 && lane.MaxStep_S < definition.Length_S)
		{
			float3 pos_I = entity.I_From_L(in pos_L);
			float3 pos_W = entity.Island.W_From_I(pos_I + new float3(0f, 0f, 0.5f));
			draw.Cross(pos_W, 0.15f, new Color(0f, 0f, 1f));
		}
		draw.Label3D((start_W + end_W) / 2f + new float3(0f, 0.75f, 0f), Quaternion.identity, definition.Name, 0.02f, LabelAlignment.Center, new Color(1f, 1f, 0f));
		draw.Label3D((start_W + end_W) / 2f + new float3(0f, 0.7f, 0f), Quaternion.identity, "Prog_T=" + lane.Progress_T + "(" + (lane.Empty ? "-" : ((lane.Progress * 100f).ToString("000") + "%")) + ")", 0.02f, LabelAlignment.Center, new Color(1f, 0f, 0f));
		draw.Label3D((start_W + end_W) / 2f + new float3(0f, 0.67f, 0f), Quaternion.identity, "Prog_S=" + lane.Definition.S_From_T(lane.Progress_T), 0.02f, LabelAlignment.Center, new Color(1f, 1f, 1f));
		draw.Label3D((start_W + end_W) / 2f + new float3(0f, 0.64f, 0f), Quaternion.identity, "MaxStp_S=" + lane.MaxStep_S, 0.02f, LabelAlignment.Center, new Color(1f, 1f, 1f));
		if (lane.HasItem)
		{
			float3 pos_I2 = entity.I_From_L(in pos_L);
			float3 pos_W2 = entity.Island.W_From_I(in pos_I2);
			draw.WireCylinder(pos_W2 + new float3(0f, 0.2f, 0f), Vector3.up, 0.1f, 0.25f, new Color(0f, 1f, 0f));
		}
	}

	public void OnGameDraw()
	{
		CommandBuilder draw = DrawingManager.GetBuilder(renderInGame: true);
		draw.cameraTargets = new Camera[1] { Singleton<GameCore>.G.LocalPlayer.Viewport.TransparentCamera };
		foreach (Island island in Singleton<GameCore>.G.LocalPlayer.CurrentMap.Islands)
		{
			foreach (MapEntity building in island.Buildings.Buildings)
			{
				if (building is BeltEntity)
				{
					BeltEntity belt = (BeltEntity)building;
					if (belt.IndexInPath == 0)
					{
						VisualizeLane(draw, belt, belt.Belts_GetLaneForInput(0));
					}
					if (belt.IndexInPath == belt.Path.Belts.Count - 1)
					{
						VisualizeLane(draw, belt, belt.Belts_GetLaneForOutput(0));
					}
				}
				else
				{
					building.Belts_TraverseLanes(new LaneVisualizer(building, draw));
				}
			}
		}
		draw.Dispose();
	}
}
