using System;
using System.Collections.Generic;
using Drawing;
using Unity.Mathematics;
using UnityEngine;

public class BeltPortSenderEntity : MapEntity<BeltPortSenderEntityMetaBuildingInternalVariant>
{
	protected class CollidedShape
	{
		public BeltItem Item;

		public double CollisionTime_G;

		public float3 CollisionStart_L;

		public float StartRotation;
	}

	public enum SenderMode
	{
		None,
		Void,
		ReceiverOtherIsland,
		ReceiverSameIsland,
		TrashSameLayer,
		Hub
	}

	public static int BELT_PORT_RANGE_TILES = 7;

	public static int BELT_PORT_PATH_LENGTH_S = BELT_PORT_RANGE_TILES * 100000;

	public static float COLLISION_ANIMATION_DURATION_SECONDS = 0.5f;

	protected static float3[] DIRECTION_TO_ROTATION = new float3[4]
	{
		new float3(0f, 0f, 1f),
		new float3(1f, 0f, 0f),
		new float3(0f, 0f, -1f),
		new float3(-1f, 0f, 0f)
	};

	protected static Dictionary<SenderMode, int> ANIMATION_CURVE_OFFSETS = new Dictionary<SenderMode, int>
	{
		{
			SenderMode.ReceiverSameIsland,
			0
		},
		{
			SenderMode.ReceiverOtherIsland,
			0
		},
		{
			SenderMode.Void,
			3
		},
		{
			SenderMode.TrashSameLayer,
			6
		}
	};

	public static int CURVE_OFFSET_COLLISION_ANIMATION = 6;

	private readonly Bounds Bounds_W;

	protected SenderMode Mode = SenderMode.None;

	protected BeltLane InputLane;

	protected BeltLane ConversionLane;

	protected MapEntity TargetEntity;

	protected float CollisionAtProgress = -1f;

	protected double LastCollisionCheckTime_G = -10000000000.0;

	protected BeltPathLogic Path;

	protected List<CollidedShape> CollidedShapes = new List<CollidedShape>();

	private int lastRenderedFrameIndex;

	protected int AnimationCurveOffset => ANIMATION_CURVE_OFFSETS[Mode];

	protected AnimationCurve AnimationHeightCurve => InternalVariant.AnimationCurves[AnimationCurveOffset].Curve;

	protected AnimationCurve AnimationItemProgressCurve => InternalVariant.AnimationCurves[AnimationCurveOffset + 2].Curve;

	public static (SenderMode, MapEntity) FindTarget(Island island, IslandTileCoordinate tile_I, Grid.Direction rotation_G)
	{
		GlobalTileCoordinate sourceTile_G = tile_I.To_G(island);
		TileDirection offset = BELT_PORT_RANGE_TILES * TileDirection.East.Rotate(rotation_G);
		GlobalTileCoordinate targetTile_G = sourceTile_G + offset;
		Island targetIsland = island.Map.GetIslandAt_G(in targetTile_G);
		if (targetIsland == null)
		{
			return (SenderMode.Void, null);
		}
		MapEntity targetEntity = island.Map.GetEntityAt_G(in targetTile_G);
		if (targetEntity is BeltPortReceiverEntity portReceiverEntity && targetEntity.Rotation_G == rotation_G)
		{
			return ((portReceiverEntity.Island == island) ? SenderMode.ReceiverSameIsland : SenderMode.ReceiverOtherIsland, portReceiverEntity);
		}
		IslandChunk chunk = island.GetChunk_G(in targetTile_G);
		if (chunk is HUBCenterIslandChunk hubChunk && hubChunk.Hub.IsValidBeltPortInput(island, tile_I, rotation_G))
		{
			return (SenderMode.Hub, hubChunk.Hub);
		}
		if (targetEntity is TrashEntity trashEntity)
		{
			return (SenderMode.TrashSameLayer, trashEntity);
		}
		return (SenderMode.None, null);
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public BeltPortSenderEntity(CtorArgs payload)
		: base(payload)
	{
		Bounds_W = GlobalTileBounds.From(base.Tile_G, base.Tile_G + BELT_PORT_RANGE_TILES * TileDirection.ByDirection(Rotation_G)).To_W();
		ConversionLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1])
		{
			PostAcceptHook = ConversionLanePostAcceptHook
		};
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], ConversionLane);
		Path = new BeltPathLogic(BELT_PORT_PATH_LENGTH_S);
		UpdateInputMaxStep();
	}

	public float RaymarchForObstacles(SenderMode mode, MapEntity targetEntity, MetaBuildingInternalVariant internalVariantHandle, Island island, IslandTileCoordinate baseTile_I, Grid.Direction rotation_G)
	{
		if (!ANIMATION_CURVE_OFFSETS.TryGetValue(mode, out var curveOffset))
		{
			return -1f;
		}
		int raymarchSteps = BELT_PORT_RANGE_TILES * 10;
		AnimationCurve heightCurve = internalVariantHandle.AnimationCurves[curveOffset].Curve;
		AnimationCurve itemProgressCurve = internalVariantHandle.AnimationCurves[curveOffset + 2].Curve;
		for (int i = 0; i < raymarchSteps; i++)
		{
			float progress = (float)i / (float)raymarchSteps;
			float3 pos_I = MapEntity.I_From_L(Position_L_FromProgress(progress, heightCurve, itemProgressCurve), rotation_G, in baseTile_I);
			IslandTileCoordinate tile_I = new IslandTileCoordinate((short)math.round(pos_I.x), (short)math.round(pos_I.y), baseTile_I.z);
			if (tile_I == baseTile_I || !island.IsValidAndFilledTile_I(in tile_I))
			{
				continue;
			}
			MapEntity entity = island.GetEntity_I(in tile_I);
			if (targetEntity != null && entity == targetEntity)
			{
				break;
			}
			if (entity == null)
			{
				continue;
			}
			float3 posOther_L = entity.L_From_I(in pos_I);
			bool colliding = false;
			MetaBuildingInternalVariant.CollisionBox[] colliders = entity.InternalVariant.Colliders;
			foreach (MetaBuildingInternalVariant.CollisionBox collider in colliders)
			{
				if (collider.Contains(posOther_L))
				{
					colliding = true;
					break;
				}
			}
			if (colliding)
			{
				float itemSize = 25000f / (float)BELT_PORT_PATH_LENGTH_S;
				return math.max(0f, progress - itemSize);
			}
		}
		return -1f;
	}

	public float3 Position_L_FromProgress(float progress, AnimationCurve heightCurve, AnimationCurve itemProgressCurve)
	{
		float itemProgress = itemProgressCurve.Evaluate(progress);
		float3 from_L = InputLane.Definition.ItemEndPos_L;
		MapEntity targetEntity = TargetEntity;
		MapEntity mapEntity = targetEntity;
		float3 to_L;
		if (mapEntity is BeltPortReceiverEntity beltPortReceiver)
		{
			float3 to_Other_L = beltPortReceiver.OutputLane.Definition.ItemStartPos_L;
			to_L = L_From_W(TargetEntity.W_From_L(in to_Other_L));
		}
		else
		{
			to_L = new float3(BELT_PORT_RANGE_TILES, 0f, 0f);
		}
		float3 position_L = math.lerp(from_L, to_L, itemProgress);
		return position_L + new float3(0f, 0f, Globals.Resources.BeltShapeHeight + heightCurve.Evaluate(itemProgress));
	}

	protected override void Hook_SyncLate(ISerializationVisitor visitor)
	{
		FindTarget();
		RaymarchForObstacles();
	}

	public override BeltItem Belts_ComputeRepresentativeShapeTransferItem()
	{
		if (Path.Items.Count > 0)
		{
			return Path.Items[0].Item;
		}
		if (InputLane.HasItem)
		{
			return InputLane.Item;
		}
		return null;
	}

	protected void ConversionLanePostAcceptHook(BeltLane lane, ref int remainingTicks_T)
	{
		switch (Mode)
		{
		case SenderMode.None:
			throw new Exception("No mode, port can not accept item");
		case SenderMode.Hub:
			((HubEntity)TargetEntity).AcceptItemFromBeltPort(lane.Item, remainingTicks_T, Tile_I, Rotation_G);
			lane.ClearLaneRaw_UNSAFE();
			UpdateInputMaxStep();
			break;
		case SenderMode.Void:
		case SenderMode.ReceiverOtherIsland:
		case SenderMode.ReceiverSameIsland:
		case SenderMode.TrashSameLayer:
		{
			int steps_S = remainingTicks_T * ConversionLane.Definition.StepsPerTick_S;
			if (Path.AcceptItem(lane.Item, steps_S, Path_ComputeMaxProgress_S()))
			{
				lane.ClearLaneRaw_UNSAFE();
				UpdateInputMaxStep();
				break;
			}
			throw new Exception("Path logic didn't accept item - but there is no reason to. (Max=" + Path_ComputeMaxProgress_S() + ")");
		}
		default:
			throw new NotImplementedException("Unsupported mode in accept.");
		}
	}

	protected void UpdateInputMaxStep()
	{
		if (ConversionLane.HasItem)
		{
			Debug.LogError("DEV ERROR: Conversion lane on belt port sender has item after update");
			ConversionLane.ClearLaneRaw_UNSAFE();
		}
		switch (Mode)
		{
		case SenderMode.None:
			ConversionLane.MaxStep_S = -1;
			break;
		case SenderMode.Hub:
			ConversionLane.MaxStep_S = 50000;
			break;
		case SenderMode.Void:
		case SenderMode.TrashSameLayer:
			ConversionLane.MaxStep_S = Path.FirstItemDistance_S - 50000;
			break;
		case SenderMode.ReceiverOtherIsland:
		case SenderMode.ReceiverSameIsland:
			if (Path.Items.Count >= 2)
			{
				ConversionLane.MaxStep_S = -1;
			}
			else
			{
				ConversionLane.MaxStep_S = Path.FirstItemDistance_S - 50000;
			}
			break;
		default:
			throw new NotImplementedException();
		}
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		Path.Sync(visitor);
	}

	protected void FindTarget()
	{
		(Mode, TargetEntity) = FindTarget(Island, Tile_I, Rotation_G);
	}

	protected void RaymarchForObstacles()
	{
		CollisionAtProgress = RaymarchForObstacles(Mode, TargetEntity, InternalVariant, Island, Tile_I, Rotation_G);
		LastCollisionCheckTime_G = Singleton<GameCore>.G.SimulationSpeed.SimulationTime_G;
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane })
		};
	}

	protected bool Path_ItemTransferHandler(BeltItem item, int excessSteps_S)
	{
		switch (Mode)
		{
		case SenderMode.Void:
		case SenderMode.TrashSameLayer:
			return true;
		case SenderMode.ReceiverOtherIsland:
		case SenderMode.ReceiverSameIsland:
		{
			int excessTicks_T = ConversionLane.Definition.T_From_S(excessSteps_S);
			return BeltSimulation.TransferToLane(item, ((BeltPortReceiverEntity)TargetEntity).OutputLane, excessTicks_T);
		}
		default:
			throw new Exception("Invalid mode for port path");
		}
	}

	protected int Path_HandlerGetMinStepsToEnd_S()
	{
		switch (Mode)
		{
		case SenderMode.Void:
		case SenderMode.TrashSameLayer:
			return -50000;
		case SenderMode.ReceiverOtherIsland:
		case SenderMode.ReceiverSameIsland:
			return -((BeltPortReceiverEntity)TargetEntity).OutputLane.MaxStep_S;
		default:
			throw new Exception("Invalid mode for port path");
		}
	}

	protected int Path_ComputeMaxProgress_S()
	{
		switch (Mode)
		{
		case SenderMode.Void:
		case SenderMode.TrashSameLayer:
			return Path.Length_S + 50000;
		case SenderMode.ReceiverOtherIsland:
		case SenderMode.ReceiverSameIsland:
			return Path.Length_S + ((BeltPortReceiverEntity)TargetEntity).OutputLane.MaxStep_S;
		default:
			throw new NotImplementedException();
		}
	}

	protected void ConvertCollidedItemsIntoAnimations(TickOptions options)
	{
		if (!(CollisionAtProgress >= 0f))
		{
			return;
		}
		while (Path.Items.Count > 0)
		{
			List<BeltPathLogic.ItemOnBelt> items = Path.Items;
			BeltPathLogic.ItemOnBelt last = items[items.Count - 1];
			int lastProgress_S = Path.Length_S - last.NextItemDistance_S;
			float lastProgress = (float)lastProgress_S / (float)Path.Length_S;
			if (lastProgress > CollisionAtProgress)
			{
				Path.RemoveLastItem();
				CollidedShapes.Add(new CollidedShape
				{
					Item = last.Item,
					CollisionTime_G = options.SimulationTime_G,
					CollisionStart_L = Position_L_FromProgress(CollisionAtProgress, AnimationHeightCurve, AnimationItemProgressCurve),
					StartRotation = InternalVariant.AnimationCurves[AnimationCurveOffset + 1].Curve.Evaluate(lastProgress)
				});
				continue;
			}
			break;
		}
	}

	protected void UpdateCollisionAnimations(TickOptions options)
	{
		for (int i = CollidedShapes.Count - 1; i >= 0; i--)
		{
			CollidedShape animation = CollidedShapes[i];
			if (options.SimulationTime_G - animation.CollisionTime_G > (double)COLLISION_ANIMATION_DURATION_SECONDS)
			{
				CollidedShapes.DeleteBySwappingWithLast_ForwardIteration(ref i);
			}
		}
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ConversionLane);
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		SenderMode oldMode = Mode;
		FindTarget();
		if (Mode != oldMode)
		{
			Path.ClearItems();
			UpdateInputMaxStep();
			RaymarchForObstacles();
		}
		else
		{
			double checkInterval = (options.LowUPS ? 10.0 : 0.5);
			if (options.SimulationTime_G - LastCollisionCheckTime_G > checkInterval)
			{
				RaymarchForObstacles();
			}
		}
		if (Mode == SenderMode.Void || Mode == SenderMode.TrashSameLayer || Mode == SenderMode.ReceiverSameIsland || Mode == SenderMode.ReceiverOtherIsland)
		{
			Path.Update(options, ConversionLane.Definition.S_From_T(options.DeltaTicks_T), endIsConnected: true, Path_ItemTransferHandler, Path_HandlerGetMinStepsToEnd_S);
		}
		ConvertCollidedItemsIntoAnimations(options);
		UpdateCollisionAnimations(options);
		UpdateInputMaxStep();
		BeltSimulation.UpdateLane(options, InputLane);
	}

	public override HashSet<MapEntity> Belts_GetDependencies()
	{
		FindTarget();
		SenderMode mode = Mode;
		SenderMode senderMode = mode;
		if ((uint)(senderMode - 2) <= 1u)
		{
			return new HashSet<MapEntity> { TargetEntity };
		}
		return new HashSet<MapEntity>();
	}

	protected override void DrawStatic_EndCaps(MeshBuilder builder)
	{
		base.DrawStatic_EndCaps(builder);
		LOD2Mesh[] standMeshes = InternalVariant.SupportMeshesInternalLOD;
		int standHeight = DrawStatic_GetStandHeight_L(in TileDirection.Zero);
		builder.AddTranslateRotate(standMeshes[math.min(1 + standHeight, standMeshes.Length - 1)], W_From_L(new float3(0)), Rotation_G);
	}

	protected void DrawItems(FrameDrawOptions options, int curveOffset, float scaleDecayWithDepth = 0f)
	{
		AnimationCurve heightCurve = InternalVariant.AnimationCurves[curveOffset].Curve;
		AnimationCurve rotationCurve = InternalVariant.AnimationCurves[curveOffset + 1].Curve;
		AnimationCurve itemProgressCurve = InternalVariant.AnimationCurves[curveOffset + 2].Curve;
		int progress_S = Path.FirstItemDistance_S;
		for (int i = 0; i < Path.Items.Count; i++)
		{
			BeltPathLogic.ItemOnBelt entry = Path.Items[i];
			float progress = (float)progress_S / (float)Path.Length_S;
			if (InternalVariant.OverwriteItemProgress)
			{
				progress = InternalVariant.ItemProgress;
			}
			BeltItem item = entry.Item;
			float rotation = rotationCurve.Evaluate(progress);
			float3 pos_W = W_From_L(Position_L_FromProgress(progress, heightCurve, itemProgressCurve));
			float3 euler = DIRECTION_TO_ROTATION[(int)Rotation_G] * 180f * rotation;
			float scale = 1.01f - math.saturate((0f - scaleDecayWithDepth) * pos_W.y);
			options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), item.GetMesh(), item.GetMaterial(), Matrix4x4.TRS(pos_W, Quaternion.Euler(euler), new float3(scale)));
			progress_S += entry.NextItemDistance_S;
		}
	}

	protected void DrawCollisionAnimations(FrameDrawOptions options)
	{
		if (CollidedShapes.Count != 0)
		{
			float speed = (float)(ConversionLane.Definition.StepsPerTick_S * IslandSimulator.UPS) / 100000f * COLLISION_ANIMATION_DURATION_SECONDS * 0.2f;
			double time_G = Singleton<GameCore>.G.SimulationSpeed.SimulationTime_G;
			for (int index = 0; index < CollidedShapes.Count; index++)
			{
				CollidedShape animation = CollidedShapes[index];
				float progress = InternalVariant.GetCurve(CURVE_OFFSET_COLLISION_ANIMATION + 2, (float)(time_G - animation.CollisionTime_G) / COLLISION_ANIMATION_DURATION_SECONDS);
				float heightOffset = InternalVariant.GetCurve(CURVE_OFFSET_COLLISION_ANIMATION, progress);
				float3 pos_W = W_From_L(animation.CollisionStart_L + new float3((0f - progress) * speed, 0f, heightOffset));
				float rotation = animation.StartRotation + InternalVariant.GetCurve(CURVE_OFFSET_COLLISION_ANIMATION + 1, progress);
				float3 euler = DIRECTION_TO_ROTATION[(int)Rotation_G] * 180f * rotation;
				float scale = math.clamp(1f - progress, 0.01f, 1f);
				BeltItem item = animation.Item;
				options.ShapeInstanceManager.AddInstance(item.GetDefaultInstancingKey(), item.GetMesh(), item.GetMaterial(), Matrix4x4.TRS(pos_W, Quaternion.Euler(euler), new float3(scale)));
			}
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic(options);
	}

	public void DrawDynamic(FrameDrawOptions options)
	{
		if (lastRenderedFrameIndex == options.FrameIndex)
		{
			return;
		}
		lastRenderedFrameIndex = options.FrameIndex;
		if (!GeometryUtility.TestPlanesAABB(options.CameraPlanes, Bounds_W))
		{
			return;
		}
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(-0.302357f, 0.28658f, 0.31541f), options.AnimationSimulationTime_G * 600f);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(-0.302357f, -0.28658f, 0.31541f), options.AnimationSimulationTime_G * -600f);
		DrawDynamic_BeltLane(options, InputLane);
		DrawCollisionAnimations(options);
		if (!ANIMATION_CURVE_OFFSETS.TryGetValue(Mode, out var curveOffset))
		{
			return;
		}
		switch (Mode)
		{
		case SenderMode.Void:
			if (InternalVariant.RenderItems)
			{
				DrawItems(options, curveOffset, 0.1f);
			}
			break;
		case SenderMode.TrashSameLayer:
			if (InternalVariant.RenderItems)
			{
				DrawItems(options, curveOffset, 0.5f);
			}
			break;
		case SenderMode.ReceiverOtherIsland:
		case SenderMode.ReceiverSameIsland:
			if (InternalVariant.RenderItems)
			{
				DrawItems(options, curveOffset);
			}
			if (InternalVariant.RenderStoppers)
			{
				DrawStoppers(options);
			}
			break;
		}
		if (InternalVariant.RenderDebugCurve)
		{
			DrawDebugItemCurve(options, curveOffset);
		}
	}

	private void DrawStoppers(FrameDrawOptions options)
	{
		int progress_S = Path.FirstItemDistance_S;
		for (int i = 0; i < Path.Items.Count - 1; i++)
		{
			BeltPathLogic.ItemOnBelt entry = Path.Items[i];
			progress_S += entry.NextItemDistance_S;
		}
		float progress = (InternalVariant.OverwriteItemProgress ? InternalVariant.ItemProgress : ((float)progress_S / (float)Path.Length_S));
		float stopperRotation = InternalVariant.StopperCurve.Evaluate(progress);
		if (InternalVariant.StopperLeft.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh stopperLeft))
		{
			float3 pos_L = InternalVariant.StopperPosition;
			pos_L.x += BELT_PORT_RANGE_TILES;
			pos_L.y *= -1f;
			float3 pos_W = W_From_L(in pos_L);
			Quaternion rotation = Quaternion.Euler(0f, Grid.DirectionToDegrees(Rotation_G), 0f);
			rotation *= Quaternion.Euler(0f, 0f, InternalVariant.StopperTilt);
			rotation *= Quaternion.Euler(0f, 90f - 180f * stopperRotation, 0f);
			options.DynamicBuildingsInstanceManager.AddInstance(stopperLeft.InstancingID, stopperLeft.Mesh, options.Theme.BaseResources.BuildingMaterial, Matrix4x4.TRS(pos_W, rotation, Vector3.one));
		}
		if (InternalVariant.StopperRight.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh stopperRight))
		{
			float3 pos_L2 = InternalVariant.StopperPosition;
			pos_L2.x += BELT_PORT_RANGE_TILES;
			float3 pos_W2 = W_From_L(in pos_L2);
			Quaternion rotation2 = Quaternion.Euler(0f, Grid.DirectionToDegrees(Rotation_G), 0f);
			rotation2 *= Quaternion.Euler(0f, 0f, InternalVariant.StopperTilt);
			rotation2 *= Quaternion.Euler(0f, -90f + 180f * stopperRotation, 0f);
			options.DynamicBuildingsInstanceManager.AddInstance(stopperRight.InstancingID, stopperRight.Mesh, options.Theme.BaseResources.BuildingMaterial, Matrix4x4.TRS(pos_W2, rotation2, Vector3.one));
		}
	}

	private void DrawDebugItemCurve(FrameDrawOptions options, int curveOffset)
	{
		using CommandBuilder draw = options.GetDebugDrawManager();
		AnimationCurve heightCurve = InternalVariant.AnimationCurves[curveOffset].Curve;
		AnimationCurve itemProgressCurve = InternalVariant.AnimationCurves[curveOffset + 2].Curve;
		float3 lastPos_W = W_From_L(Position_L_FromProgress(0f, heightCurve, itemProgressCurve));
		int progressSteps = 0;
		while (progressSteps++ < 100)
		{
			float3 pos_W = W_From_L(Position_L_FromProgress((float)progressSteps / 100f, heightCurve, itemProgressCurve));
			draw.Line(lastPos_W, pos_W, Color.green);
			lastPos_W = pos_W;
		}
	}
}
