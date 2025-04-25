using System;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable]
public class FluidUnpackerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane ProcessingLane;

	protected FluidContainer Container;

	public FluidUnpackerEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], ProcessingLane);
		ProcessingLane.PreAcceptHook = delegate(BeltItem item)
		{
			FluidCrateItem fluidCrateItem = (FluidCrateItem)item;
			if (Container.Value > Container.Max - FluidCrateItem.FLUID_CAPACITY)
			{
				return (BeltItem)null;
			}
			return (Container.Fluid != null && Container.Fluid != fluidCrateItem.StoredFluid) ? null : item;
		};
		ProcessingLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			FluidCrateItem fluidCrateItem = (FluidCrateItem)lane.Item;
			Container.Add(FluidCrateItem.FLUID_CAPACITY, fluidCrateItem.StoredFluid);
			lane.ClearLaneRaw_UNSAFE();
			lane.MaxStep_S = lane.Definition.Length_S;
		};
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return Container;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[3]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleFluidContainerContents(Container, 1.8181819f),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, ProcessingLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration + internalVariant.BeltLaneDefinitions[1].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[2]
		{
			MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)),
			new HUDSidePanelModuleStatFluidCrateSize(FluidCrateItem.FLUID_CAPACITY)
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, ProcessingLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		Fluid fluid = Container.Fluid;
		if (fluid != null && InternalVariant.SupportMeshesInternalLOD[1].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh mesh))
		{
			float fluidScale = math.clamp(Container.Level / 0.2f, 0.2f, 1f);
			options.RegularRenderer.DrawMesh(mesh, FastMatrix.TranslateScale(W_From_L(new float3(0f, 0f, 0.21f)), new float3(fluidScale, Container.Level, fluidScale)), fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}
}
