using Unity.Mathematics;
using UnityEngine;

public class TopmostPainterEntity : PainterEntity
{
	private const float RollRadius = 0.3f;

	private static float TotalShapeHeight(BeltItem shape)
	{
		int shapeLayerCount = ((ShapeItem)shape).Definition.Layers.Length;
		return TotalShapeHeight(shapeLayerCount);
	}

	private static float TotalShapeHeight(int shapeLayerCount)
	{
		return (float)shapeLayerCount * Globals.Resources.ShapeLayerHeight;
	}

	public TopmostPainterEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override BeltItem PaintItem(ShapeDefinition shape, MetaShapeColor color)
	{
		string result = Singleton<GameCore>.G.Shapes.Op_PaintTopmost.Execute(new ShapeOperationPaintTopmostPayload(shape, color));
		return Singleton<GameCore>.G.Shapes.GetItemByHash(result);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		float rollIdleHeight = 0.3f + TotalShapeHeight(4);
		float rollItemHeight = (ProcessingLane.HasItem ? (0.3f + TotalShapeHeight(ProcessingLane.Item)) : rollIdleHeight);
		float itemProgressNormalized = InternalVariant.GetCurve(0, ProcessingLane.Progress);
		float rollAngleNormalized = InternalVariant.GetCurve(1, ProcessingLane.Progress);
		float pitchAngleNormalized = InternalVariant.GetCurve(2, ProcessingLane.Progress);
		float rollHeightBlend = InternalVariant.GetCurve(3, ProcessingLane.Progress);
		float rollHeight = math.lerp(rollIdleHeight, rollItemHeight, rollHeightBlend);
		if (ProcessingLane.HasItem)
		{
			DrawDynamic_BeltItem(options, ProcessingLane.Item, math.lerp(ProcessingLane.Definition.ItemStartPos_L, ProcessingLane.Definition.ItemEndPos_L, itemProgressNormalized));
		}
		if (InternalVariant.SupportMeshesInternalLOD[1].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh rollMesh))
		{
			options.RegularRenderer.DrawMesh(rollMesh, Matrix4x4.TRS(W_From_L(new float3(0f, -0.578789f, rollHeight)), Quaternion.Euler(0f, Grid.DirectionToDegrees(Rotation_G), 0f) * Quaternion.Euler(-180f * rollAngleNormalized, 0f, pitchAngleNormalized * 360f), new float3(1f, 1f, 1f)), options.Theme.BaseResources.BuildingMaterial, RenderCategory.BuildingsDynamic);
		}
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[2], new float3(0f, -0.585441f, rollHeight));
		Fluid fluid = Container.Fluid;
		if (fluid != null && InternalVariant.SupportMeshesInternalLOD[0].TryGet(options.BuildingsLOD, out LODBaseMesh.CachedMesh fluidMesh))
		{
			options.RegularRenderer.DrawMesh(fluidMesh, Matrix4x4.TRS(W_From_L(new float3(0f, 0f, 0.15f)), FastMatrix.RotateY(Rotation_G), new float3(1f, Container.Level, 1f)), fluid.GetMaterial(), RenderCategory.Fluids);
		}
	}
}
