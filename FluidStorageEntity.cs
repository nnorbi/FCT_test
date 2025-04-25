using Unity.Mathematics;
using UnityEngine;

public class FluidStorageEntity : PipeEntity
{
	public FluidStorageEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		if (Container.Fluid != null && !(Container.Level < 0.01f) && InternalVariant.SupportMeshesInternalLOD[0].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh contentsMesh))
		{
			options.RegularRenderer.DrawMesh(contentsMesh, Matrix4x4.TRS(W_From_L(new float3(0f, 0f, 0f)), FastMatrix.RotateY(Rotation_G), new float3(1f, Container.Level, 1f)), Container.Fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}
}
