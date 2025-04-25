using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class PinPusherEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane ProcessingLane;

	protected BeltLane OutputLane;

	protected ShapeItem CurrentWaste;

	protected ShapeCollapseResult CurrentResultWithoutPin;

	public PinPusherEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1], OutputLane);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], ProcessingLane);
		ProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			ShapeDefinition definition = (lane.Item as ShapeItem).Definition;
			PushPinOperationResult pushPinOperationResult = Singleton<GameCore>.G.Shapes.Op_PushPin.Execute(definition);
			CurrentWaste = Singleton<GameCore>.G.Shapes.GetItemByHash(pushPinOperationResult.Waste);
			CurrentResultWithoutPin = pushPinOperationResult.ResultWithoutPin;
			lane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(pushPinOperationResult.ResultWithPin);
		};
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		BeltItem.Sync(visitor, ref CurrentWaste);
		ShapeCollapseResult.Sync(visitor, ref CurrentResultWithoutPin);
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, ProcessingLane, OutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration + internalVariant.BeltLaneDefinitions[1].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, ProcessingLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, OutputLane);
		float platformHeight = 0f;
		float pinHeight = -0.1f;
		GameResources resources = Globals.Resources;
		if (ProcessingLane.HasItem)
		{
			float progress = ProcessingLane.Progress;
			platformHeight = InternalVariant.GetCurve(0, progress) * 0.1f;
			pinHeight = InternalVariant.GetCurve(1, progress) * 0.2f + platformHeight;
			float collapseProgress = InternalVariant.GetCurve(5, progress);
			if (CurrentResultWithoutPin != null)
			{
				float baseHeight = Globals.Resources.BeltShapeHeight + platformHeight;
				DrawDynamic_ShapeCollapseResult(options, CurrentResultWithoutPin, new float3(0f, 0f, baseHeight + Globals.Resources.ShapeSupportHeight), collapseProgress, collapseProgress, collapseProgress);
			}
			CachedInstancingMesh supportMesh = ShapeItem.SUPPORT_MESH;
			options.ShapeInstanceManager.AddInstance(supportMesh.InstancingID, supportMesh.Mesh, Globals.Resources.ShapeMaterial, FastMatrix.Translate(W_From_L(new float3(0f, 0f, platformHeight + Globals.Resources.BeltShapeHeight))));
			if (CurrentWaste != null)
			{
				float wasteHeight = InternalVariant.GetCurve(2, progress) * 0.5f;
				float wasteSideOffset = InternalVariant.GetCurve(3, progress) * -0.5f;
				float wasteOpacity = InternalVariant.GetCurve(4, progress);
				options.RegularRenderer.DrawMesh(CurrentWaste.Definition.GetMesh(), FastMatrix.Translate(W_From_L(new float3(0f, wasteSideOffset, resources.BeltShapeHeight + resources.ShapeSupportHeight + wasteHeight))), resources.ShapeMaterialFadeout, RenderCategory.Shapes, MaterialPropertyHelpers.CreateAlphaBlock(wasteOpacity));
			}
		}
		ShapeDefinition pinDefinition = Singleton<GameCore>.G.Shapes.GetDefinitionByHash("P-------");
		Mesh pinMesh = pinDefinition.GetMesh();
		options.RegularRenderer.DrawMesh(pinMesh, FastMatrix.Translate(W_From_L(new float3(0f, 0f, resources.BeltShapeHeight + resources.ShapeSupportHeight + pinHeight))), resources.ShapeMaterial, RenderCategory.Shapes);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[((int)Rotation_G % 2 == 0) ? 1u : 0u], new float3(0f, 0f, 0f), Grid.RotateDirection(Grid.InvertDirection(Rotation_G), Grid.Direction.Top));
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[2], new float3(0f, 0f, 0.13f + platformHeight));
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[3], new float3(0f, 0f, 0.13f + platformHeight));
	}
}
