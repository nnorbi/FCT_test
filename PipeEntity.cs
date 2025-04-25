using Unity.Mathematics;
using UnityEngine;

public class PipeEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected FluidContainer Container;

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1]
		{
			new HUDSidePanelModuleStatFluidCapacity(internalVariant.FluidContainers[0].Max)
		};
	}

	public PipeEntity(CtorArgs payload)
		: base(payload)
	{
		Container = new FluidContainer(this, InternalVariant.FluidContainers[0]);
	}

	public override Config_UpdateMode Order_ComputeUpdateMode()
	{
		return Config_UpdateMode.Never;
	}

	public override FluidContainer Fluids_GetContainerByIndex(int n)
	{
		return Container;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[1]
		{
			new HUDSidePanelModuleFluidContainerContents(Container)
		};
	}

	protected virtual void Draw_PipeFluidMesh(FrameDrawOptions options, int maxMeshes)
	{
		float height = Container.Level;
		Fluid fluid = Container.Fluid;
		if (options.BuildingsLOD >= 2)
		{
			height = ((height > 0.01f) ? 1f : 0f);
		}
		if (!(height < 0.01f) && fluid != null)
		{
			int meshIndex = math.clamp((int)math.round(height * (float)maxMeshes), 0, maxMeshes - 1);
			if (InternalVariant.SupportMeshesInternalLOD.Length > meshIndex && InternalVariant.SupportMeshesInternalLOD[meshIndex].TryGet(math.min(options.BuildingsLOD, 1), out LODBaseMesh.CachedMesh fluidMesh))
			{
				InstancedMeshManager fluidsInstanceManager = options.FluidsInstanceManager;
				Mesh mesh = fluidMesh;
				Matrix4x4 transform = FastMatrix.TranslateRotate(W_From_L(new float3(0f, 0f, 0.2f)), Rotation_G);
				fluidsInstanceManager.AddInstanceSlow(mesh, fluid.GetMaterial(), in transform);
			}
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		Draw_PipeFluidMesh(options, InternalVariant.SupportMeshesInternalLOD.Length);
	}
}
