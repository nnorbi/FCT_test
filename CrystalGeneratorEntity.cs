using System;
using System.Collections.Generic;

[Serializable]
public class CrystalGeneratorEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected static float LITERS_PER_ITEM = 10f;

	protected BeltLane InputLane;

	protected BeltLane OutputLane;

	protected FluidContainer Container;

	public CrystalGeneratorEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], OutputLane);
		OutputLane.PreAcceptHook = (BeltItem item) => (Container.Fluid == null || !(Container.Fluid is ColorFluid) || Container.Value < LITERS_PER_ITEM) ? null : item;
		OutputLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			ShapeDefinition definition = ((ShapeItem)lane.Item).Definition;
			ColorFluid colorFluid = (ColorFluid)Container.Fluid;
			string hash = Singleton<GameCore>.G.Shapes.Op_GenerateCrystal.Execute(new ShapeOperationGenerateCrystalPayload
			{
				Shape = definition,
				Color = colorFluid.Color
			});
			lane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(hash);
			Container.Take(LITERS_PER_ITEM);
		};
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return Container;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, OutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, OutputLane);
	}
}
