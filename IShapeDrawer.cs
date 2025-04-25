using UnityEngine;

public interface IShapeDrawer
{
	void DrawShape(FrameDrawOptions options, ShapeDefinition definition, Matrix4x4 transform);
}
