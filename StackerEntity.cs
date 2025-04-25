using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class StackerEntity : MapEntity<StackerEntityMetaBuildingInternalVariant>
{
	protected BeltLane LowerInputLane;

	protected BeltLane UpperInputLane;

	protected BeltLane ProcessingLane;

	protected BeltLane OutputLane;

	protected ShapeCollapseResult CurrentCollapseResult;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[1].Duration + internalVariant.BeltLaneDefinitions[2].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public StackerEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[3]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2], OutputLane);
		UpperInputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
		LowerInputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1], ProcessingLane);
		UpperInputLane.MaxStepClampHook = (BeltLane lane, int maxStep_S) => (!ProcessingLane.HasItem) ? lane.Definition.Length_S : 0;
		LowerInputLane.MaxStepClampHook = (BeltLane lane, int maxStep_S) => (!ProcessingLane.HasItem) ? maxStep_S : 0;
		ProcessingLane.PreAcceptHook = (BeltItem item) => (!UpperInputLane.HasItem || UpperInputLane.Progress_T < UpperInputLane.Definition.Duration_T - 1) ? null : item;
		ProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			ShapeStackResult shapeStackResult = Singleton<GameCore>.G.Shapes.Op_Stack.Execute(new ShapeOperationStackPayload
			{
				LowerShape = (LowerInputLane.Item as ShapeItem).Definition,
				UpperShape = (UpperInputLane.Item as ShapeItem).Definition
			});
			CurrentCollapseResult = shapeStackResult.Result;
			UpperInputLane.ClearLaneRaw_UNSAFE();
			UpperInputLane.MaxStep_S = UpperInputLane.Definition.Length_S;
			lane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(CurrentCollapseResult.ResultDefinition);
		};
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		ShapeCollapseResult.Sync(visitor, ref CurrentCollapseResult);
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		if (1 == 0)
		{
		}
		BeltLane result = index switch
		{
			0 => LowerInputLane, 
			1 => UpperInputLane, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, ProcessingLane);
		BeltSimulation.UpdateLane(options, UpperInputLane);
		BeltSimulation.UpdateLane(options, LowerInputLane);
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, ProcessingLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { LowerInputLane, UpperInputLane, ProcessingLane, OutputLane })
		};
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		if (InternalVariant.RenderLaneItems)
		{
			DrawDynamic_BeltLane(options, UpperInputLane);
			DrawDynamic_BeltLane(options, LowerInputLane);
			DrawDynamic_BeltLane(options, OutputLane);
		}
		float progress = (ProcessingLane.HasItem ? (ProcessingLane.Progress * 1.5f) : 0f);
		if (InternalVariant.RenderMolds)
		{
			RenderMolds(options, progress);
		}
		if (InternalVariant.RenderLids)
		{
			RenderLids(options, progress);
		}
		if (ProcessingLane.HasItem && CurrentCollapseResult != null)
		{
			if (InternalVariant.RenderShapePlatforms)
			{
				RenderShapePlatforms(options, progress);
			}
			if (InternalVariant.RenderShapes)
			{
				RenderShapes(options, progress);
			}
		}
	}

	private void RenderShapePlatforms(FrameDrawOptions options, float progress)
	{
		float upperShapeHeight = InternalVariant.UpperShapeYOffsetCurve.Evaluate(progress);
		CachedInstancingMesh supportMesh = ShapeItem.SUPPORT_MESH;
		options.ShapeInstanceManager.AddInstance(supportMesh.InstancingID, supportMesh.Mesh, Globals.Resources.ShapeMaterial, FastMatrix.Translate(W_From_L(new float3(0f, 0f, Globals.Resources.BeltShapeHeight))));
		float upperFloorAlpha = InternalVariant.UpperFloorAlphaCurve.Evaluate(progress);
		if (upperFloorAlpha > 0.01f)
		{
			MaterialPropertyBlock upperFloorAlphaBlock = MaterialPropertyHelpers.CreateAlphaBlock(upperFloorAlpha);
			float3 upper_W = W_From_L(new float3(0f, 0f, upperShapeHeight) + new float3(0f, 0f, Globals.Resources.BeltShapeHeight));
			options.RegularRenderer.DrawMesh(supportMesh.Mesh, FastMatrix.Translate(in upper_W), (upperFloorAlpha > 0.99f) ? Globals.Resources.ShapeMaterial : Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, upperFloorAlphaBlock);
		}
	}

	private void RenderShapes(FrameDrawOptions options, float progress)
	{
		float upperShapeHeight = InternalVariant.UpperShapeYOffsetCurve.Evaluate(progress);
		float upperShapeProgressFactor = 1f - upperShapeHeight;
		float upperShapeScaleXFactor = InternalVariant.UpperShapeScaleYCurve.Evaluate(progress);
		float upperShapeScaleYFactor = InternalVariant.UpperShapeScaleXCurve.Evaluate(progress);
		if (Rotation_G == Grid.Direction.Bottom || Rotation_G == Grid.Direction.Top)
		{
			float num = upperShapeScaleYFactor;
			float num2 = upperShapeScaleXFactor;
			upperShapeScaleXFactor = num;
			upperShapeScaleYFactor = num2;
		}
		float3 pos_W = W_From_L(new float3(0f, 0f, Globals.Resources.BeltShapeHeight + Globals.Resources.ShapeSupportHeight));
		int baseLayer = Singleton<GameCore>.G.Mode.MaxShapeLayers + 1;
		float sourceScaleReduction = ShapeLogic.Logic_LayerScale(baseLayer);
		float wasteAlpha = InternalVariant.WasteAlphaCurve.Evaluate(progress);
		MaterialPropertyBlock wastePropertyBlock = MaterialPropertyHelpers.CreateAlphaBlock(wasteAlpha);
		for (int i = 0; i < CurrentCollapseResult.Entries.Length; i++)
		{
			ShapeCollapseResultEntry entry = CurrentCollapseResult.Entries[i];
			float sourceScale = ShapeLogic.Logic_LayerScale(entry.FallDownLayers);
			float sourceHeightOffset = Globals.Resources.ShapeLayerHeight * (float)entry.FallDownLayers;
			if (entry.FallDownLayers != 0)
			{
				sourceScale /= sourceScaleReduction;
				int layerDifference = entry.FallDownLayers - baseLayer;
				sourceHeightOffset = 1f + (float)layerDifference * Globals.Resources.ShapeLayerHeight;
			}
			float scaleX = math.lerp(sourceScale, 1f, upperShapeScaleXFactor);
			float scaleY = math.lerp(sourceScale, 1f, upperShapeScaleYFactor);
			float heightOffset = math.lerp(sourceHeightOffset, 0f, upperShapeProgressFactor);
			ShapeDefinition definition = Singleton<GameCore>.G.Shapes.GetDefinitionByHash(entry.ShapeDefinition);
			Matrix4x4 transform = FastMatrix.TranslateScale(pos_W + new float3(0f, heightOffset, 0f), new float3(scaleX, 1f, scaleY));
			if (entry.Vanish)
			{
				if (wasteAlpha > 0.005f)
				{
					options.RegularRenderer.DrawMesh(definition.GetMesh(), in transform, Globals.Resources.ShapeMaterialFadeout, RenderCategory.Shapes, wastePropertyBlock);
				}
			}
			else
			{
				options.ShapeInstanceManager.AddInstanceSlow(definition.GetMesh(), Globals.Resources.ShapeMaterial, in transform);
			}
		}
	}

	private void RenderMolds(FrameDrawOptions options, float progress)
	{
		float moldsDistance = InternalVariant.MoldsDistanceCurve.Evaluate(progress);
		float leftRotationDeg = InternalVariant.MoldLeftRotationCurve.Evaluate(progress) * 360f;
		float2 leftOffset = Grid.Rotate(InternalVariant.MoldsOffset.xy, leftRotationDeg) * (1f - moldsDistance);
		DrawDynamic_Mesh(options, InternalVariant.MoldLeftMesh, new float3(leftOffset, InternalVariant.MoldsOffset.z), leftRotationDeg);
		float rightRotationDeg = InternalVariant.MoldRightRotationCurve.Evaluate(progress) * 360f;
		float2 rightOffset = Grid.Rotate(InternalVariant.MoldsOffset.xy, rightRotationDeg) * (1f - moldsDistance);
		DrawDynamic_Mesh(options, InternalVariant.MoldRightMesh, new float3(-rightOffset, InternalVariant.MoldsOffset.z), rightRotationDeg);
	}

	private void RenderLids(FrameDrawOptions options, float progress)
	{
		float lidFactor = InternalVariant.LidFactorCurve.Evaluate(progress);
		if (InternalVariant.LidLeftMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh lidLeftMesh))
		{
			options.RegularRenderer.DrawMesh(lidLeftMesh, (Matrix4x4)math.mul(FastMatrix.Translate(W_From_L(new float3(0f, -0.2058f, 1.1908f - lidFactor * 0f))), math.mul(float4x4.RotateY(math.radians(Grid.DirectionToDegrees(Rotation_G))), float4x4.RotateX(math.radians((0f - lidFactor) * 90f)))), options.Theme.BaseResources.BuildingMaterial, RenderCategory.BuildingsDynamic);
		}
		if (InternalVariant.LidRightMesh.TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh lidRightMesh))
		{
			options.RegularRenderer.DrawMesh(lidRightMesh, (Matrix4x4)math.mul(FastMatrix.Translate(W_From_L(new float3(0f, 0.2058f, 1.1908f - lidFactor * 0f))), math.mul(float4x4.RotateY(math.radians(Grid.DirectionToDegrees(Rotation_G))), float4x4.RotateX(math.radians(lidFactor * 90f)))), options.Theme.BaseResources.BuildingMaterial, RenderCategory.BuildingsDynamic);
		}
	}
}
