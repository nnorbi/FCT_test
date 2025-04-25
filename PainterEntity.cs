using System.Collections.Generic;

public abstract class PainterEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected static float LITERS_PER_ITEM = 10f;

	protected BeltLane ProcessingLane;

	protected Fluid CurrentProcessingPaint = null;

	protected FluidContainer Container;

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[2]
		{
			MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)),
			new HUDSidePanelModuleStatLitersPerItem(LITERS_PER_ITEM)
		};
	}

	protected PainterEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
		ProcessingLane.PreAcceptHook = (BeltItem item) => (Container.Value < LITERS_PER_ITEM) ? null : item;
		ProcessingLane.PostAcceptHook = delegate
		{
			CurrentProcessingPaint = Container.Fluid;
			Container.Take(LITERS_PER_ITEM);
		};
	}

	protected abstract BeltItem PaintItem(ShapeDefinition shape, MetaShapeColor color);

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		Fluid.Sync(visitor, ref CurrentProcessingPaint);
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return Container;
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return ProcessingLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return ProcessingLane;
	}

	public override void Belts_TraverseLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		float duration = ProcessingLane.Definition.Duration;
		return new HUDSidePanelModule[3]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, ProcessingLane),
			new HUDSidePanelModuleFluidContainerContents(Container, (0f - LITERS_PER_ITEM) / duration),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { ProcessingLane })
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, ProcessingLane);
		if (CurrentProcessingPaint != null && (double)ProcessingLane.Progress > 0.5)
		{
			ProcessingLane.Item = PaintItem(((ShapeItem)ProcessingLane.Item).Definition, ((ColorFluid)CurrentProcessingPaint).Color);
			CurrentProcessingPaint = null;
		}
	}
}
