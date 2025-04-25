using Unity.Mathematics;
using UnityEngine;

public class FullPainterEntity : PainterEntity
{
	public FullPainterEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override BeltItem PaintItem(ShapeDefinition shape, MetaShapeColor color)
	{
		string result = Singleton<GameCore>.G.Shapes.Op_Paint.Execute(new ShapeOperationPaintPayload(shape, color));
		return Singleton<GameCore>.G.Shapes.GetItemByHash(result);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		if (ProcessingLane.HasItem)
		{
			float itemProgress = InternalVariant.GetCurve(0, ProcessingLane.Progress);
			DrawDynamic_BeltItem(options, ProcessingLane.Item, new float3(-0.5f + itemProgress, -1f, 0f));
		}
		float wallHeight = InternalVariant.GetCurve(1, ProcessingLane.Progress);
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], W_From_L(new float3(0f, -1f, 0f)), new float3(1f, 1f, wallHeight));
		Fluid fluid = Container.Fluid;
		if (fluid != null)
		{
			if (InternalVariant.SupportMeshesInternalLOD[1].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh painterFluidMesh))
			{
				float painterFluidHeight = InternalVariant.GetCurve(2, ProcessingLane.Progress);
				options.RegularRenderer.DrawMesh(painterFluidMesh, Matrix4x4.TRS(W_From_L(new float3(0f, -1f, 0f)), FastMatrix.RotateY(Rotation_G), new float3(1f, painterFluidHeight, 1f)), fluid.GetMaterial(), RenderCategory.Fluids);
			}
			float pipeFluidHeight = InternalVariant.GetCurve(3, ProcessingLane.Progress);
			if (InternalVariant.SupportMeshesInternalLOD[1].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh pipeFluidMesh))
			{
				options.RegularRenderer.DrawMesh(pipeFluidMesh, Matrix4x4.TRS(W_From_L(new float3(0f, -0.5f, 0f)), FastMatrix.RotateY(Rotation_G), new float3(1f, pipeFluidHeight, 1f)), fluid.GetMaterial(), RenderCategory.Fluids);
			}
			if (InternalVariant.SupportMeshesInternalLOD[2].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh containerFluidMesh))
			{
				options.RegularRenderer.DrawMesh(containerFluidMesh, Matrix4x4.TRS(W_From_L(new float3(0f, 0f, 0f)), FastMatrix.RotateY(Rotation_G), new float3(1f, Container.Level, 1f)), fluid.GetMaterial(), RenderCategory.Fluids);
			}
		}
	}
}
