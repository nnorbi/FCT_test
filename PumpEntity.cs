using Unity.Mathematics;
using UnityEngine;

public class PumpEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected static float PUMP_FLOW_PER_SECOND = 20f;

	protected FluidContainer Container;

	protected Fluid Fluid;

	protected float CurrentRotation = 0f;

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1]
		{
			new HUDSidePanelModuleStatFluidProduction(PUMP_FLOW_PER_SECOND * 60f)
		};
	}

	public PumpEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return Container;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[1]
		{
			new HUDSidePanelModuleFluidContainerContents(Container, PUMP_FLOW_PER_SECOND)
		};
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		if (Fluid != null)
		{
			if (Container.Fluid != Fluid)
			{
				Debug.LogWarning("Fluid container fluid mismatch, flushing");
				Container.Flush();
			}
			if (Container.ConnectedContainers.Count > 0)
			{
				float delta = Container.GetSignedFlowRateAtIndex(0);
				CurrentRotation += delta * 360f * options.DeltaTime;
			}
			float fillRate = options.DeltaTime * PUMP_FLOW_PER_SECOND;
			Container.AddAndClamp(fillRate, Fluid);
		}
	}

	protected override void Hook_OnCreated()
	{
		Fluid = Island.GetTileInfo_UNSAFE_I(in Tile_I).FluidResource;
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		if (Fluid != null)
		{
			DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0f, 0f, (Container.Level - 1f) * 0.3f), CurrentRotation);
			if (InternalVariant.SupportMeshesInternalLOD[1].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh mesh1))
			{
				options.RegularRenderer.DrawMesh(mesh1, FastMatrix.Translate(W_From_L(new float3(0))), Fluid.GetMaterial(), RenderCategory.BuildingsDynamic);
			}
			if (InternalVariant.SupportMeshesInternalLOD[1].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh mesh2))
			{
				options.RegularRenderer.DrawMesh(mesh2, FastMatrix.TranslateScale(W_From_L(new float3(0)), new float3(1f, 0.1f + 0.9f * Container.Level, 1f)), Fluid.GetMaterial(), RenderCategory.Fluids);
			}
		}
	}
}
