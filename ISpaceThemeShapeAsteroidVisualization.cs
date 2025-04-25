using UnityEngine;

public interface ISpaceThemeShapeAsteroidVisualization
{
	void Add(LODBaseMesh mesh, Matrix4x4 transform);

	void Draw(FrameDrawOptions options);
}
