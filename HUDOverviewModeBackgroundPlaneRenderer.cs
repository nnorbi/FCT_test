using Unity.Mathematics;
using UnityEngine;

public class HUDOverviewModeBackgroundPlaneRenderer
{
	private Mesh BackgroundPlaneMesh;

	public HUDOverviewModeBackgroundPlaneRenderer()
	{
		BackgroundPlaneMesh = GeometryHelpers.GetPlaneMesh_CACHED(default(Color));
	}

	public void Draw(FrameDrawOptions options, float alpha)
	{
		options.RegularRenderer.DrawMesh(BackgroundPlaneMesh, FastMatrix.TranslateScale((float3)new Vector3(options.CameraPosition_W.x, -3f, options.CameraPosition_W.z), new float3(60000f, 1f, 60000f)), options.Theme.BaseResources.UXOverviewModeBackgroundPlaneMaterial, RenderCategory.AnalogUI, MaterialPropertyHelpers.CreateAlphaBlock(alpha));
	}
}
