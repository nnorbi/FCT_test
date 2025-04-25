using System;
using System.Collections.Generic;
using Unity.Mathematics;

public class CutterEntity : MapEntity<CutterEntityMetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane LeftLane;

	protected BeltLane RightLane;

	protected BeltLane LeftOutputLane;

	protected BeltLane RightOutputLane;

	protected ShapeCollapseResult LeftCollapseResult;

	protected ShapeCollapseResult RightCollapseResult;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration + internalVariant.BeltLaneDefinitions[1].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public CutterEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition[] definitions = InternalVariant.BeltLaneDefinitions;
		RightOutputLane = new BeltLane(definitions[4]);
		RightLane = new BeltLane(definitions[3], RightOutputLane);
		LeftOutputLane = new BeltLane(definitions[2]);
		LeftLane = new BeltLane(definitions[1], LeftOutputLane);
		InputLane = new BeltLane(definitions[0], RightLane);
		InputLane.MaxStepClampHook = delegate(BeltLane lane, int maxStep_S)
		{
			if (LeftOutputLane.HasItem)
			{
				maxStep_S = math.min(maxStep_S, LeftOutputLane.MaxStep_S + 50000);
			}
			if (RightOutputLane.HasItem)
			{
				maxStep_S = math.min(maxStep_S, RightOutputLane.MaxStep_S + 50000);
			}
			if (LeftLane.HasItem || RightLane.HasItem)
			{
				maxStep_S = math.min(maxStep_S, 0);
			}
			return math.max(0, maxStep_S);
		};
		RightLane.PreAcceptHook = (BeltItem item) => (RightOutputLane.HasItem || LeftOutputLane.HasItem || LeftLane.HasItem || RightLane.HasItem) ? null : item;
		RightLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			if (LeftLane.HasItem)
			{
				throw new Exception("Cutter left lane still has item during right post accept hook");
			}
			ShapeItem shapeItem = RightLane.Item as ShapeItem;
			ShapeCutResult shapeCutResult = Singleton<GameCore>.G.Shapes.Op_Cut.Execute(shapeItem.Definition);
			ShapeItem itemByHash = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeCutResult.LeftSide?.ResultDefinition);
			ShapeItem itemByHash2 = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeCutResult.RightSide?.ResultDefinition);
			LeftCollapseResult = shapeCutResult.LeftSide;
			RightCollapseResult = shapeCutResult.RightSide;
			if (itemByHash != null || itemByHash2 != null)
			{
				if (itemByHash != null)
				{
					LeftLane.Item = itemByHash;
					LeftLane.Progress_T = remainingTicks_T;
					BeltSimulation.UpdateLaneMaxProgressWithItem(LeftLane);
				}
				if (itemByHash2 != null)
				{
					RightLane.Item = itemByHash2;
					BeltSimulation.UpdateLaneMaxProgressWithItem(RightLane);
				}
				else
				{
					if (!RightLane.HasItem)
					{
						throw new Exception("Cutter right lane has no item in post accept hook");
					}
					RightLane.ClearLane();
				}
			}
		};
		RightOutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			if (RightCollapseResult.ResultsInEmptyShape)
			{
				lane.ClearLane();
			}
		};
		LeftOutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			if (LeftCollapseResult.ResultsInEmptyShape)
			{
				lane.ClearLane();
			}
		};
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		ShapeCollapseResult.Sync(visitor, ref LeftCollapseResult);
		ShapeCollapseResult.Sync(visitor, ref RightCollapseResult);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		if (index == 0)
		{
			return LeftOutputLane;
		}
		return RightOutputLane;
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(LeftLane);
		traverser.Traverse(RightLane);
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, LeftOutputLane, RightOutputLane, LeftLane, RightLane })
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, LeftOutputLane);
		BeltSimulation.UpdateLane(options, RightOutputLane);
		BeltSimulation.UpdateLane(options, LeftLane);
		BeltSimulation.UpdateLane(options, RightLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, LeftOutputLane);
		DrawDynamic_BeltLane(options, RightOutputLane);
		float progress = 0f;
		bool processing = false;
		if (LeftLane.HasItem)
		{
			processing = true;
			progress = LeftLane.Progress;
		}
		else if (RightLane.HasItem)
		{
			processing = true;
			progress = RightLane.Progress;
		}
		int adjustedProgress_T = math.clamp(LeftLane.Definition.ProgressToTicks_UNSAFE(InternalVariant.ProgressAdjustmentCurve.Evaluate(progress)), 0, LeftLane.Definition.Duration_T - 1);
		float3 posLeft_L = LeftLane.Definition.GetPosFromTicks_L(LeftLane.HasItem ? adjustedProgress_T : 0);
		float3 posRight_L = RightLane.Definition.GetPosFromTicks_L(adjustedProgress_T);
		float ribbonHeightBlend = InternalVariant.RibbonHeightBlendCurve.Evaluate(progress);
		float ribbonIdleHeight = Globals.Resources.BeltShapeHeight + InternalVariant.RibbonIdleOffset;
		float ribbonProcessHeight = (LeftLane.HasItem ? (Globals.Resources.BeltShapeHeight + LeftLane.Item.ItemHeight + InternalVariant.RibbonHeightOffset) : ribbonIdleHeight);
		float ribbonHeight = math.lerp(ribbonIdleHeight, ribbonProcessHeight, ribbonHeightBlend);
		float collapseProgress = InternalVariant.ShapeCutCollapseCurve.Evaluate(progress);
		float distanceProgress = InternalVariant.ShapeCutDistanceCurve.Evaluate(progress);
		float3 distanceVector_L = new float3(Grid.Rotate(new float2(0.1f * distanceProgress, 0f), Grid.DirectionToDegrees(Grid.OppositeDirection(Grid.InvertDirection(Rotation_G)))), 0f);
		if (InternalVariant.RenderStaticSemicircles)
		{
			RenderStaticSemicircles(options);
		}
		if (InternalVariant.RenderLaserCutter)
		{
			RenderLaserCutter(options, progress);
		}
		if (InternalVariant.RenderRibbon)
		{
			RenderRibbon(options, ribbonHeight);
		}
		if (InternalVariant.RenderDynamicPlatform)
		{
			RenderDynamicPlatform(options, posLeft_L, ribbonHeight);
		}
		if (InternalVariant.RenderNewElements)
		{
			RenderNewElements(options, processing, progress, posLeft_L, distanceVector_L, ribbonHeight);
		}
		if (InternalVariant.RenderLeftShape)
		{
			RenderLeftShape(options, processing, posLeft_L, distanceVector_L, ribbonHeight, collapseProgress);
		}
		if (InternalVariant.RenderRightShape)
		{
			RenderRightShape(options, processing, progress, posRight_L, distanceVector_L, collapseProgress);
		}
	}

	private void RenderStaticSemicircles(FrameDrawOptions options)
	{
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.StaticSemicircleMesh, (float3)InternalVariant.Tiles[1] + new float3(0f, 0f, Globals.Resources.BeltShapeHeight));
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.StaticSemicircleMesh, (float3)InternalVariant.Tiles[0] + new float3(0f, 0f, Globals.Resources.BeltShapeHeight));
		DrawDynamic_MeshGlobalRotation(options, InternalVariant.IndicatorMesh, (float3)InternalVariant.Tiles[1] + new float3(0f, 0f, InternalVariant.StaticIndicatorHeight), Grid.Direction.Left);
	}

	private void RenderLaserCutter(FrameDrawOptions options, float progress)
	{
		float laserHeight = InternalVariant.LaserHeightCurve.Evaluate(progress);
		if (laserHeight > 0.01f && InternalVariant.LaserCutterMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh laserMesh))
		{
			options.DynamicBuildingsInstanceManager.AddInstance(laserMesh, options.Theme.BaseResources.BuildingLaserMaterial, FastMatrix.TranslateScale(W_From_L(new float3(0f, 0f, 0.19f)), Grid.Scale_W_From_G(new float3(1f, 1f, 2.5f * laserHeight))));
		}
	}

	private void RenderRibbon(FrameDrawOptions options, float ribbonHeight)
	{
		float3 buildingCenter_W = 0.5f * (float3)(InternalVariant.Tiles[0] + InternalVariant.Tiles[1]);
		DrawDynamic_Mesh(options, InternalVariant.RibbonMesh, buildingCenter_W + new float3(0f, 0f, ribbonHeight));
		DrawDynamic_Mesh(options, InternalVariant.RibbonBaseMesh, in buildingCenter_W, new float3(1f, 1f, ribbonHeight));
	}

	private void RenderDynamicPlatform(FrameDrawOptions options, float3 posLeft_L, float ribbonHeight)
	{
		float beltHeight = Globals.Resources.BeltShapeHeight;
		DrawDynamic_Mesh(options, InternalVariant.CircleArcMesh, posLeft_L + new float3(0f, 0f, math.max(beltHeight, ribbonHeight)), 2 - Rotation_G);
		DrawDynamic_Mesh(options, InternalVariant.IndicatorMesh, posLeft_L + new float3(0f, 0f, math.max(beltHeight, ribbonHeight) + InternalVariant.IndicatorHeightOffset), 2 - Rotation_G);
		if (LeftOutputLane.HasItem)
		{
			float oldElementRotation = InternalVariant.OldElementRotationCurve.Evaluate(LeftOutputLane.Progress) * 360f;
			DrawDynamic_Mesh(options, InternalVariant.CircleArcMesh, (float3)InternalVariant.Tiles[1] + new float3(0f, 0f, math.max(beltHeight, ribbonHeight)), 0f - Grid.DirectionToDegrees(Rotation_G) + oldElementRotation);
			DrawDynamic_Mesh(options, InternalVariant.IndicatorMesh, (float3)InternalVariant.Tiles[1] + new float3(0f, 0f, math.max(beltHeight, ribbonHeight) + InternalVariant.IndicatorHeightOffset), 0f - Grid.DirectionToDegrees(Rotation_G) + oldElementRotation);
		}
	}

	private void RenderNewElements(FrameDrawOptions options, bool processing, float progress, float3 posLeft_L, float3 distanceVector_L, float ribbonHeight)
	{
		if (!processing)
		{
			return;
		}
		float newElementRotation = InternalVariant.NewElementRotationCurve.Evaluate(progress) * 360f;
		if (!(newElementRotation <= 0f))
		{
			ref float beltHeight = ref Globals.Resources.BeltShapeHeight;
			if (LeftLane.HasItem && RightLane.HasItem)
			{
				DrawDynamic_LeftShapeSupportMeshRaw(options, FastMatrix.TranslateRotateDegrees(W_From_L(-distanceVector_L + new float3(0f, 0f, beltHeight)), 180f + newElementRotation));
			}
			if (LeftLane.HasItem)
			{
				DrawDynamic_RightShapeSupportMeshRaw(options, FastMatrix.TranslateRotateDegrees(W_From_L(posLeft_L + distanceVector_L + new float3(0f, 0f, math.max(beltHeight, ribbonHeight))), 180f + newElementRotation));
			}
			DrawDynamic_MeshGlobalRotation(options, InternalVariant.CircleArcMesh, new float3(0f, 0f, beltHeight), newElementRotation);
			DrawDynamic_MeshGlobalRotation(options, InternalVariant.IndicatorMesh, new float3(0f, 0f, beltHeight + InternalVariant.IndicatorHeightOffset), newElementRotation);
		}
	}

	private void RenderLeftShape(FrameDrawOptions options, bool processing, float3 posLeft_L, float3 distanceVector_L, float ribbonHeight, float collapseProgress)
	{
		if (processing)
		{
			ref float beltHeight = ref Globals.Resources.BeltShapeHeight;
			DrawDynamic_LeftShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posLeft_L + distanceVector_L + new float3(0f, 0f, math.max(beltHeight, ribbonHeight)))));
			if (LeftCollapseResult != null)
			{
				DrawDynamic_ShapeCollapseResult(options, LeftCollapseResult, posLeft_L + distanceVector_L + new float3(0f, 0f, Globals.Resources.ShapeSupportHeight + math.max(beltHeight, ribbonHeight)), collapseProgress, collapseProgress, collapseProgress);
			}
		}
	}

	private void RenderRightShape(FrameDrawOptions options, bool processing, float progress, float3 posRight_L, float3 distanceVector_L, float collapseProgress)
	{
		ref float beltHeight = ref Globals.Resources.BeltShapeHeight;
		ref float supportHeight = ref Globals.Resources.ShapeSupportHeight;
		if (RightCollapseResult != null && processing)
		{
			DrawDynamic_ShapeCollapseResult(options, RightCollapseResult, posRight_L - distanceVector_L + new float3(0f, 0f, beltHeight + supportHeight), collapseProgress, collapseProgress, collapseProgress);
		}
		if (RightLane.HasItem)
		{
			float rightSupportAlpha = 1f;
			if (RightCollapseResult.ResultsInEmptyShape)
			{
				rightSupportAlpha = math.saturate(1f - 3f * progress);
			}
			DrawDynamic_RightShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posRight_L - distanceVector_L + new float3(0f, 0f, beltHeight))), rightSupportAlpha);
		}
		else if (LeftLane.HasItem)
		{
			float rightAlpha = math.saturate(1f - 5f * progress);
			DrawDynamic_RightShapeSupportMeshRaw(options, FastMatrix.Translate(W_From_L(posRight_L - distanceVector_L + new float3(0f, 0f, beltHeight))), rightAlpha);
		}
	}
}
