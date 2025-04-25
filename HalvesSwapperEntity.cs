using System.Collections.Generic;
using Unity.Mathematics;

public class HalvesSwapperEntity : MapEntity<MetaBuildingInternalVariant>
{
	private const float RIBBON_HEIGHT_OFFSET = 0.1f;

	private const float UPPER_RIBBON_HEIGHT_OFFSET = 0.1f;

	private const float LOWER_RIBBON_IDLE_HEIGHT = -0.1f;

	private const int PROGRESS_FACTOR_CURVE_INDEX = 0;

	private const int SAW_HEIGHT_CURVE_INDEX = 1;

	private const int PLATFORM_ALPHA_FADEOUT_INDEX = 2;

	private const int COLLAPSE_PROGRESS_CURVE_INDEX = 3;

	private const int DISTANCE_PROGRESS_CURVE_INDEX = 4;

	private const int LOWER_RIBBON_HEIGHT_BLEND_CURVE_INDEX = 5;

	private const int UPPER_RIBBON_HEIGHT_BLEND_CURVE_INDEX = 6;

	protected BeltLane LowerInputLane;

	protected BeltLane LowerProcessingLane;

	protected BeltLane LowerOutputLane;

	protected BeltLane UpperInputLane;

	protected BeltLane UpperProcessingLane;

	protected BeltLane UpperOutputLane;

	protected ShapeCollapseResult LowerLeftCollapseResult;

	protected ShapeCollapseResult LowerRightCollapseResult;

	protected ShapeCollapseResult UpperLeftCollapseResult;

	protected ShapeCollapseResult UpperRightCollapseResult;

	protected ShapeItem LowerFinalResult;

	protected ShapeItem UpperFinalResult;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration + internalVariant.BeltLaneDefinitions[1].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public HalvesSwapperEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition[] definitions = InternalVariant.BeltLaneDefinitions;
		LowerOutputLane = new BeltLane(definitions[2]);
		LowerProcessingLane = new BeltLane(definitions[1], LowerOutputLane);
		LowerInputLane = new BeltLane(definitions[0], LowerProcessingLane);
		UpperOutputLane = new BeltLane(definitions[5]);
		UpperProcessingLane = new BeltLane(definitions[4], UpperOutputLane);
		UpperInputLane = new BeltLane(definitions[3], UpperProcessingLane);
		InitLaneHooks();
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		ShapeCollapseResult.Sync(visitor, ref LowerLeftCollapseResult);
		ShapeCollapseResult.Sync(visitor, ref LowerRightCollapseResult);
		ShapeCollapseResult.Sync(visitor, ref UpperLeftCollapseResult);
		ShapeCollapseResult.Sync(visitor, ref UpperRightCollapseResult);
		BeltItem.Sync(visitor, ref LowerFinalResult);
		BeltItem.Sync(visitor, ref UpperFinalResult);
	}

	protected void InitLaneHooks()
	{
		LowerInputLane.MaxStepClampHook = delegate(BeltLane lane, int maxStep_S)
		{
			if (LowerProcessingLane.HasItem)
			{
				return 0;
			}
			return LowerOutputLane.HasItem ? math.min(LowerOutputLane.MaxStep_S + 50000, maxStep_S) : maxStep_S;
		};
		UpperInputLane.MaxStepClampHook = delegate(BeltLane lane, int maxStep_S)
		{
			if (UpperProcessingLane.HasItem)
			{
				return 0;
			}
			return UpperOutputLane.HasItem ? math.min(UpperOutputLane.MaxStep_S + 50000, maxStep_S) : maxStep_S;
		};
		UpperProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			if (LowerProcessingLane.HasItem)
			{
				remainingTicks_T = math.min(remainingTicks_T, LowerInputLane.Progress_T);
				PrepareProcessing();
			}
		};
		LowerProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			if (UpperProcessingLane.HasItem)
			{
				remainingTicks_T = math.min(remainingTicks_T, UpperInputLane.Progress_T);
				PrepareProcessing();
			}
		};
		UpperOutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			UpperLeftCollapseResult = null;
			UpperRightCollapseResult = null;
			lane.Item = UpperFinalResult;
			if (lane.Item == null)
			{
				lane.MaxStep_S = lane.ComputeMaxStepWhenEmptyINTERNAL_S();
			}
			UpperFinalResult = null;
		};
		LowerOutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			LowerLeftCollapseResult = null;
			LowerRightCollapseResult = null;
			lane.Item = LowerFinalResult;
			if (lane.Item == null)
			{
				lane.MaxStep_S = lane.ComputeMaxStepWhenEmptyINTERNAL_S();
			}
			LowerFinalResult = null;
		};
	}

	protected void PrepareProcessing()
	{
		ShapeHalvesSwapResult result = Singleton<GameCore>.G.Shapes.Op_SwapHalves.Execute(new ShapeOperationSwapHalvesPayload
		{
			LowerShape = ((ShapeItem)LowerProcessingLane.Item).Definition,
			UpperShape = ((ShapeItem)UpperProcessingLane.Item).Definition
		});
		LowerLeftCollapseResult = result.LowerLeftCollapseResult;
		LowerRightCollapseResult = result.LowerRightCollapseResult;
		UpperLeftCollapseResult = result.UpperLeftCollapseResult;
		UpperRightCollapseResult = result.UpperRightCollapseResult;
		LowerFinalResult = Singleton<GameCore>.G.Shapes.GetItemByHash(result.LowerFinalResult);
		UpperFinalResult = Singleton<GameCore>.G.Shapes.GetItemByHash(result.UpperFinalResult);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return (index == 0) ? LowerInputLane : UpperInputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return (index == 0) ? LowerOutputLane : UpperOutputLane;
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(LowerProcessingLane);
		traverser.Traverse(UpperProcessingLane);
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, LowerProcessingLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { LowerInputLane, LowerProcessingLane, LowerOutputLane, UpperInputLane, UpperProcessingLane, UpperOutputLane })
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, LowerOutputLane);
		BeltSimulation.UpdateLane(options, UpperOutputLane);
		BeltSimulation.UpdateLane(options, LowerProcessingLane);
		BeltSimulation.UpdateLane(options, UpperProcessingLane);
		if (LowerProcessingLane.HasItem && UpperProcessingLane.HasItem)
		{
			int progress = math.min(LowerProcessingLane.Progress_T, UpperProcessingLane.Progress_T);
			LowerProcessingLane.Progress_T = progress;
			UpperProcessingLane.Progress_T = progress;
		}
		BeltSimulation.UpdateLane(options, LowerInputLane);
		BeltSimulation.UpdateLane(options, UpperInputLane);
		if (LowerProcessingLane.HasItem && !UpperProcessingLane.HasItem)
		{
			LowerProcessingLane.Progress_T = 0;
		}
		else if (UpperProcessingLane.HasItem && !LowerProcessingLane.HasItem)
		{
			UpperProcessingLane.Progress_T = 0;
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, LowerOutputLane);
		DrawDynamic_BeltLane(options, UpperOutputLane);
		DrawDynamic_BeltLane(options, LowerInputLane);
		DrawDynamic_BeltLane(options, UpperInputLane);
		float progress = 0f;
		if (LowerProcessingLane.HasItem && UpperProcessingLane.HasItem)
		{
			progress = LowerProcessingLane.Progress;
		}
		if (LowerProcessingLane.HasItem && !UpperProcessingLane.HasItem)
		{
			DrawDynamic_BeltLane(options, LowerProcessingLane);
		}
		else if (UpperProcessingLane.HasItem && !LowerProcessingLane.HasItem)
		{
			DrawDynamic_BeltLane(options, UpperProcessingLane);
		}
		GameResources resources = Globals.Resources;
		int adjustedProgress_T = math.clamp(UpperProcessingLane.Definition.ProgressToTicks_UNSAFE(InternalVariant.GetCurve(0, progress)), 0, UpperProcessingLane.Definition.Duration_T - 1);
		float3 posUpperLeft_L = UpperProcessingLane.Definition.GetPosFromTicks_L(UpperProcessingLane.HasItem ? adjustedProgress_T : 0);
		float3 posUpperRight_L = UpperOutputLane.Definition.GetPosFromTicks_L(0);
		float3 posLowerLeft_L = LowerProcessingLane.Definition.GetPosFromTicks_L(LowerProcessingLane.HasItem ? adjustedProgress_T : 0);
		float3 posLowerRight_L = LowerOutputLane.Definition.GetPosFromTicks_L(0);
		float supportHeight = resources.ShapeSupportHeight;
		float beltHeight = resources.BeltShapeHeight;
		if (InternalVariant.SupportMeshesInternalLOD[0].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh laserMesh))
		{
			float sawHeight = InternalVariant.GetCurve(1, progress);
			if (sawHeight > 0.01f)
			{
				options.DynamicBuildingsInstanceManager.AddInstance(laserMesh, options.Theme.BaseResources.BuildingLaserMaterial, FastMatrix.TranslateScale(W_From_L(new float3(0f, -1f, 0.19f)), Grid.Scale_W_From_G(new float3(1f, 1f, 2.5f * sawHeight))));
				options.DynamicBuildingsInstanceManager.AddInstance(laserMesh, options.Theme.BaseResources.BuildingLaserMaterial, FastMatrix.TranslateScale(W_From_L(new float3(0f, 0f, 0.19f)), Grid.Scale_W_From_G(new float3(1f, 1f, 2.5f * sawHeight))));
			}
		}
		float lowerRibbonHeightBlend = InternalVariant.GetCurve(5, progress);
		float upperRibbonHeightBlend = InternalVariant.GetCurve(6, progress);
		float lowerItemHeight = (LowerProcessingLane.HasItem ? LowerProcessingLane.Item.ItemHeight : 0f);
		float upperItemHeight = (UpperProcessingLane.HasItem ? UpperProcessingLane.Item.ItemHeight : 0f);
		float maxItemHeight = math.max(lowerItemHeight, upperItemHeight);
		float lowerRibbonIdleHeight = beltHeight + -0.1f;
		float lowerRibbonProcessHeight = (UpperProcessingLane.HasItem ? (beltHeight + maxItemHeight + 0.1f) : lowerRibbonIdleHeight);
		float lowerRibbonHeight = math.lerp(lowerRibbonIdleHeight, lowerRibbonProcessHeight, lowerRibbonHeightBlend);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[4], new float3(0f, 0f, lowerRibbonHeight));
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[5], new float3(0f, -0.5f, 0f), new float3(1f, 1f, lowerRibbonHeight));
		float upperRibbonHeight = math.lerp(lowerRibbonHeight + 0.1f, lowerRibbonHeight + lowerItemHeight + 0.1f + 0.1f, upperRibbonHeightBlend);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[4], new float3(0f, 0f, upperRibbonHeight));
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[6], new float3(0f, -0.5f, 0f), new float3(1f, 1f, upperRibbonHeight));
		float collapseProgress = InternalVariant.GetCurve(3, progress);
		float distanceProgress = InternalVariant.GetCurve(4, progress);
		float3 distanceVector_L = new float3(Grid.Rotate(new float2(0.1f * distanceProgress, 0f), Grid.DirectionToDegrees(Grid.OppositeDirection(Grid.InvertDirection(Rotation_G)))), 0f);
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.SupportMeshesInternalLOD[1], new float3(0f, -1f, beltHeight));
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.SupportMeshesInternalLOD[1], new float3(0f, 0f, beltHeight));
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.SupportMeshesInternalLOD[7], posUpperLeft_L + distanceVector_L + new float3(0f, 0f, math.max(beltHeight, upperRibbonHeight)), Grid.Direction.Left);
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.SupportMeshesInternalLOD[7], posLowerLeft_L + distanceVector_L + new float3(0f, 0f, math.max(beltHeight, lowerRibbonHeight)), Grid.Direction.Left);
		float platformFadeoutAlpha = InternalVariant.GetCurve(2, progress);
		float lowerAlpha = ((LowerFinalResult == null) ? platformFadeoutAlpha : 1f);
		float upperAlpha = ((UpperFinalResult == null) ? platformFadeoutAlpha : 1f);
		if (LowerProcessingLane.HasItem)
		{
			if (LowerLeftCollapseResult != null)
			{
				DrawDynamic_ShapeCollapseResult(options, LowerLeftCollapseResult, posLowerLeft_L + distanceVector_L + new float3(0f, 0f, supportHeight + math.max(beltHeight, lowerRibbonHeight)), collapseProgress, collapseProgress, collapseProgress);
			}
			DrawDynamic_LeftShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posLowerLeft_L + distanceVector_L + new float3(0f, 0f, math.max(beltHeight, lowerRibbonHeight)))), upperAlpha);
			if (LowerRightCollapseResult != null)
			{
				DrawDynamic_ShapeCollapseResult(options, LowerRightCollapseResult, posLowerRight_L - distanceVector_L + new float3(0f, 0f, beltHeight + supportHeight), collapseProgress, collapseProgress, collapseProgress);
			}
			DrawDynamic_RightShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posLowerRight_L - distanceVector_L + new float3(0f, 0f, beltHeight))), lowerAlpha);
		}
		if (UpperProcessingLane.HasItem)
		{
			if (UpperLeftCollapseResult != null)
			{
				DrawDynamic_ShapeCollapseResult(options, UpperLeftCollapseResult, posUpperLeft_L + distanceVector_L + new float3(0f, 0f, supportHeight + math.max(beltHeight, upperRibbonHeight)), collapseProgress, collapseProgress, collapseProgress);
			}
			DrawDynamic_LeftShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posUpperLeft_L + distanceVector_L + new float3(0f, 0f, math.max(beltHeight, upperRibbonHeight)))), lowerAlpha);
			if (UpperRightCollapseResult != null)
			{
				DrawDynamic_ShapeCollapseResult(options, UpperRightCollapseResult, posUpperRight_L - distanceVector_L + new float3(0f, 0f, beltHeight + supportHeight), collapseProgress, collapseProgress, collapseProgress);
			}
			DrawDynamic_RightShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posUpperRight_L - distanceVector_L + new float3(0f, 0f, beltHeight))), upperAlpha);
		}
	}
}
