using System.Collections.Generic;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class BeltPortReceiverEntity : MapEntity<BeltPortReceiverEntityMetaBuildingInternalVariant>
{
	private static Vector3[] DIRECTION_TO_ROTATION = new Vector3[4]
	{
		new Vector3(0f, 0f, 1f),
		new Vector3(1f, 0f, 0f),
		new Vector3(0f, 0f, -1f),
		new Vector3(-1f, 0f, 0f)
	};

	public BeltLane OutputLane;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	private static bool TryFindSender(Island island, IslandTileCoordinate tile_I, Grid.Direction rotation_G, out BeltPortSenderEntity senderEntity)
	{
		GlobalTileCoordinate sourceTile_G = tile_I.To_G(island);
		TileDirection offset = BeltPortSenderEntity.BELT_PORT_RANGE_TILES * TileDirection.East.Rotate(rotation_G);
		GlobalTileCoordinate targetTile_G = sourceTile_G - offset;
		Island targetIsland = island.Map.GetIslandAt_G(in targetTile_G);
		if (targetIsland == null)
		{
			senderEntity = null;
			return false;
		}
		MapEntity targetEntity = island.Map.GetEntityAt_G(in targetTile_G);
		if (targetEntity is BeltPortSenderEntity portSenderEntity && targetEntity.Rotation_G == rotation_G)
		{
			senderEntity = portSenderEntity;
			return true;
		}
		senderEntity = null;
		return false;
	}

	public BeltPortReceiverEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, OutputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { OutputLane })
		};
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
	}

	protected override void DrawStatic_EndCaps(MeshBuilder builder)
	{
		base.DrawStatic_EndCaps(builder);
		LOD2Mesh[] standMeshes = InternalVariant.SupportMeshesInternalLOD;
		int standHeight = DrawStatic_GetStandHeight_L(in TileDirection.Zero);
		builder.AddTranslateRotate(standMeshes[math.min(1 + standHeight, standMeshes.Length - 1)], W_From_L(new float3(0)), Rotation_G);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		float progress = (InternalVariant.OverwriteItemProgress ? InternalVariant.ItemProgress : OutputLane.Progress);
		if (InternalVariant.RenderItem)
		{
			DrawItem(options, progress);
		}
		if (InternalVariant.RenderDebugCurve)
		{
			DrawDebugItemCurve(options);
		}
		if (TryFindSender(out var senderEntity))
		{
			senderEntity.DrawDynamic(options);
		}
		else if (InternalVariant.RenderStoppers)
		{
			DrawStoppers(options, 0f);
		}
	}

	private void DrawItem(FrameDrawOptions options, float progress)
	{
		if (OutputLane.HasItem)
		{
			float itemRotation = InternalVariant.ItemRotationCurve.Evaluate(progress);
			float3 pos_W = Position_W_FromProgress(progress);
			Vector3 euler = DIRECTION_TO_ROTATION[(int)Rotation_G] * 180f * itemRotation;
			options.ShapeInstanceManager.AddInstance(OutputLane.Item.GetDefaultInstancingKey(), OutputLane.Item.GetMesh(), OutputLane.Item.GetMaterial(), Matrix4x4.TRS(pos_W, Quaternion.Euler(euler), Vector3.one));
		}
	}

	private void DrawStoppers(FrameDrawOptions options, float progress)
	{
		float stopperRotation = InternalVariant.StopperCurve.Evaluate(progress);
		if (InternalVariant.StopperLeft.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh stopperLeft))
		{
			float3 pos_L = InternalVariant.StopperPosition;
			pos_L.y *= -1f;
			float3 pos_W = W_From_L(in pos_L);
			Quaternion rotation = Quaternion.Euler(0f, Grid.DirectionToDegrees(Rotation_G), 0f);
			rotation *= Quaternion.Euler(0f, 0f, InternalVariant.StopperTilt);
			rotation *= Quaternion.Euler(0f, 90f - 180f * stopperRotation, 0f);
			options.DynamicBuildingsInstanceManager.AddInstance(stopperLeft.InstancingID, stopperLeft, options.Theme.BaseResources.BuildingMaterial, Matrix4x4.TRS(pos_W, rotation, Vector3.one));
		}
		if (InternalVariant.StopperRight.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh stopperRight))
		{
			float3 pos_L2 = InternalVariant.StopperPosition;
			float3 pos_W2 = W_From_L(in pos_L2);
			Quaternion rotation2 = Quaternion.Euler(0f, Grid.DirectionToDegrees(Rotation_G), 0f);
			rotation2 *= Quaternion.Euler(0f, 0f, InternalVariant.StopperTilt);
			rotation2 *= Quaternion.Euler(0f, -90f + 180f * stopperRotation, 0f);
			options.DynamicBuildingsInstanceManager.AddInstance(stopperRight.InstancingID, stopperRight, options.Theme.BaseResources.BuildingMaterial, Matrix4x4.TRS(pos_W2, rotation2, Vector3.one));
		}
	}

	private void DrawDebugItemCurve(FrameDrawOptions options)
	{
		using CommandBuilder draw = options.GetDebugDrawManager();
		float3 lastPos_W = Position_W_FromProgress(0f);
		int progressSteps = 0;
		while (progressSteps++ < 100)
		{
			float3 pos_W = Position_W_FromProgress((float)progressSteps / 100f);
			draw.Line(lastPos_W, pos_W, Color.green);
			lastPos_W = pos_W;
		}
	}

	private float3 Position_W_FromProgress(float progress)
	{
		float3 pos_L = math.lerp(OutputLane.Definition.ItemStartPos_L, OutputLane.Definition.ItemEndPos_L, progress);
		pos_L.z = Globals.Resources.BeltShapeHeight + InternalVariant.ItemHeightCurve.Evaluate(progress);
		return W_From_L(in pos_L);
	}

	private bool TryFindSender(out BeltPortSenderEntity senderEntity)
	{
		return TryFindSender(Island, Tile_I, Rotation_G, out senderEntity);
	}
}
