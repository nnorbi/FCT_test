using UnityEngine;

public struct CustomDissolveShapeDrawer : IShapeDrawer
{
	private readonly float WasteOpacity;

	public static CustomDissolveShapeDrawer WithOpacity(float wasteOpacity)
	{
		return new CustomDissolveShapeDrawer(wasteOpacity);
	}

	private CustomDissolveShapeDrawer(float wasteOpacity)
	{
		WasteOpacity = wasteOpacity;
	}

	public void DrawShape(FrameDrawOptions options, ShapeDefinition definition, Matrix4x4 transform)
	{
		MaterialPropertyBlock wastePropertyBlock = MaterialPropertyHelpers.CreateAlphaBlock(WasteOpacity);
		options.RegularRenderer.DrawMesh(definition.GetMesh(), in transform, Globals.Resources.ShapeMaterialDissolve, RenderCategory.BuildingsDynamic, wastePropertyBlock);
	}
}
