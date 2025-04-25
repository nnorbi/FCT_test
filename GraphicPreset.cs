public class GraphicPreset
{
	public string Id;

	public GraphicsAntialiasingQuality AntialiasingQuality;

	public GraphicsAOQuality AmbientOcclusion;

	public GraphicsShadowQuality ShadowQuality;

	public GraphicsBackgroundDetails BackgroundDetails;

	public GraphicsIslandDetails IslandDetails;

	public GraphicsShaderQuality ShaderQuality;

	public GraphicsBuildingDetails BuildingDetails;

	public bool AnisotropicFiltering;

	public string Title => ("menu.settings.graphic-preset." + Id).tr();

	public string Description => ("menu.settings.graphic-preset." + Id + ".description").tr();

	public bool IsActive(GraphicSettings settings)
	{
		return settings.Antialiasing.Value == AntialiasingQuality && settings.AmbientOcclusion.Value == AmbientOcclusion && settings.ShadowQuality.Value == ShadowQuality && settings.BackgroundDetails.Value == BackgroundDetails && settings.IslandDetails.Value == IslandDetails && settings.ShaderQuality.Value == ShaderQuality && settings.BuildingDetails.Value == BuildingDetails && (bool)settings.AnisotropicFiltering == AnisotropicFiltering;
	}
}
