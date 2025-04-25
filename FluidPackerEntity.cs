using System;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable]
public class FluidPackerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane OutputLane;

	protected FluidContainer Container;

	public FluidPackerEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
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
			new HUDSidePanelModuleFluidContainerContents(Container, -1.8181819f),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { OutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
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
		BeltSimulation.UpdateLane(options, OutputLane);
		if (!OutputLane.HasItem && OutputLane.MaxStep_S >= 0 && Container.Value >= FluidCrateItem.FLUID_CAPACITY)
		{
			Container.Take(FluidCrateItem.FLUID_CAPACITY);
			OutputLane.Item = Singleton<GameCore>.G.CrateItems.GetFluidCrate(Container.Fluid);
			OutputLane.Progress_T = 0;
			OutputLane.MaxStep_S = 0;
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, OutputLane);
		Fluid fluid = Container.Fluid;
		if (fluid != null && InternalVariant.SupportMeshesInternalLOD[0].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh fluidMesh))
		{
			float fluidScale = math.clamp(Container.Level / 0.2f, 0.2f, 1f);
			options.RegularRenderer.DrawMesh(fluidMesh, FastMatrix.TranslateScale(W_From_L(new float3(0f, 0f, 1.2122f)), new float3(fluidScale, Container.Level, fluidScale)), fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}
}
