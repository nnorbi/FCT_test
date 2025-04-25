using System.Collections.Generic;

public class Lift2LayerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane VerticalLane0;

	protected BeltLane VerticalLane1;

	protected BeltLane OutputLane;

	protected int CurrentAnimationSide = 0;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	public Lift2LayerEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition[] definitions = InternalVariant.BeltLaneDefinitions;
		OutputLane = new BeltLane(definitions[3]);
		VerticalLane1 = new BeltLane(definitions[2], OutputLane);
		VerticalLane0 = new BeltLane(definitions[1], VerticalLane1);
		InputLane = new BeltLane(definitions[0], VerticalLane0);
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(VerticalLane0);
		traverser.Traverse(VerticalLane1);
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
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, VerticalLane0, VerticalLane1, OutputLane })
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, VerticalLane1);
		BeltSimulation.UpdateLane(options, VerticalLane0);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, OutputLane);
		DrawDynamic_BeltLane(options, VerticalLane1);
		DrawDynamic_BeltLane(options, VerticalLane0);
		DrawDynamic_BeltLane(options, InputLane);
	}
}
