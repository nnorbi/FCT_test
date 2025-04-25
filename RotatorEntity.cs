using System;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable]
public abstract class RotatorEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane ProcessingLane;

	protected BeltLane OutputLane;

	protected float RotatorOffset = 0f;

	public RotatorEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1], OutputLane);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], ProcessingLane);
		ProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			ShapeDefinition definition = (lane.Item as ShapeItem).Definition;
			RotatorOffset += (float)GetRotationOffset(definition) / (float)definition.PartCount;
			string hash = Singleton<GameCore>.G.Shapes.Op_Rotate.Execute(new ShapeOperationRotatePayload
			{
				Shape = definition,
				AmountClockwise = GetRotationOffset(definition)
			});
			lane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(hash);
		};
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	protected abstract int GetRotationOffset(ShapeDefinition definition);

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
		float platformRotation = RotatorOffset * 360f;
		DrawDynamic_BeltLane(options, InputLane);
		if (ProcessingLane.Item != null)
		{
			ShapeDefinition definition = (ProcessingLane.Item as ShapeItem).Definition;
			int parts = definition.PartCount;
			float rotationFactor = 1f - InternalVariant.GetCurve(0, ProcessingLane.Progress);
			float stepDegrees = -360f / (float)parts;
			float rotation = (float)GetRotationOffset(definition) * stepDegrees * rotationFactor;
			platformRotation += rotation;
			DrawDynamic_BeltLane(options, ProcessingLane, rotation);
		}
		DrawDynamic_BeltLane(options, OutputLane);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0f, 0f, 0f), platformRotation);
	}
}
