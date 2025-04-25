using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct InstancedShapeDrawer : IShapeDrawer
{
	public static readonly InstancedShapeDrawer Default;

	public void DrawShape(FrameDrawOptions options, ShapeDefinition definition, Matrix4x4 transform)
	{
		options.ShapeInstanceManager.AddInstance(definition.InstancingID, definition.GetMesh(), Globals.Resources.ShapeMaterial, in transform);
	}
}
