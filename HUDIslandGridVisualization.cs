using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;

public class HUDIslandGridVisualization : HUDVisualization
{
	protected Mesh GridMesh;

	public HUDIslandGridVisualization()
	{
		GridMesh = GeometryHelpers.GetPlaneMesh_CACHED(new Color(1f, 1f, 1f, 1f));
	}

	public override string GetGlobalIconId()
	{
		return "visualization-island-grid";
	}

	public override string GetTitle()
	{
		return "visualizations.island-grid.title".tr();
	}

	protected override float GetAnimationDuration(bool fadingOut)
	{
		return fadingOut ? 0.5f : 0.6f;
	}

	protected override Ease GetAnimationEasing(bool fadingOut)
	{
		return Ease.OutExpo;
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions options)
	{
		if (Alpha > 0.001f)
		{
			DrawGrid(options);
		}
	}

	public override bool IsAvailable()
	{
		return Player.Viewport.Scope == GameScope.Islands;
	}

	public override bool IsForcedActive()
	{
		if (Player.Viewport.Scope == GameScope.Overview)
		{
			return true;
		}
		if (Player.Viewport.Scope == GameScope.Islands && (Player.SelectedIslandLayout.Value != null || Player.CurrentBlueprint.Value is IslandBlueprint))
		{
			return true;
		}
		return false;
	}

	protected void DrawGrid(FrameDrawOptions options)
	{
		MaterialPropertyBlock propertyBlock = MaterialPropertyHelpers.CreateAlphaBlock(Alpha);
		options.AnalogUIRenderer.DrawMesh(GridMesh, FastMatrix.TranslateScale((float3)new Vector3(options.CameraPosition_W.x, 0.1f, options.CameraPosition_W.z), new float3(60000f, 1f, 60000f)), options.Theme.BaseResources.UXIslandGridVisualizationGridMaterial, RenderCategory.AnalogUI, propertyBlock);
		if (options.Viewport.Scope != GameScope.Overview)
		{
			options.RegularRenderer.DrawMesh(GridMesh, FastMatrix.TranslateScale((float3)new Vector3(options.CameraPosition_W.x, -0.2f, options.CameraPosition_W.z), new float3(60000f, 1f, 60000f)), options.Theme.BaseResources.UXIslandGridVisualizationLowerGroundMaterial, RenderCategory.AnalogUI, propertyBlock);
		}
	}
}
