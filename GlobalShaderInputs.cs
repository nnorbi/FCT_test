using UnityEngine;

public static class GlobalShaderInputs
{
	public static int PlacingBuilding = Shader.PropertyToID("_G_PlacingBuilding");

	public static int PlacingIsland = Shader.PropertyToID("_G_PlacingIsland");

	public static int TimeScale = Shader.PropertyToID("_G_TimeScale");

	public static int GlobalSimulationTime = Shader.PropertyToID("_G_GlobalSimulationTime");

	public static int CursorWorldPos = Shader.PropertyToID("_G_CursorWorldPos");

	public static int Zoom = Shader.PropertyToID("_G_Zoom");

	public static int CurrentLayerColor = Shader.PropertyToID("_G_CurrentLayerColor");

	public static int IslandOverviewPlayingfieldColor = Shader.PropertyToID("_G_IslandOverviewPlayingfieldColor");

	public static int LayersParams = Shader.PropertyToID("_G_LayersParams");

	public static int LayerColorTexture = Shader.PropertyToID("_G_LayerColorTexture");

	public static int BeltArrowColorTexture = Shader.PropertyToID("_G_BeltArrowColorTexture");

	public static int LayerCount = Shader.PropertyToID("_G_LayerCount");

	public static int MousePosition = Shader.PropertyToID("_G_MousePosition");

	public static int ChunkGridSizeOuter = Shader.PropertyToID("_G_ChunkGridSizeOuter");

	public static int CameraAngle = Shader.PropertyToID("_G_CameraAngle");

	public static int MaterialDebugMask = Shader.PropertyToID("_G_MaterialDebugBitSet");

	public static int MaterialDebugPulseBitMask = Shader.PropertyToID("_G_MaterialDebugPulseBitMask");

	public static int MaterialDebugPulseStartTime = Shader.PropertyToID("_G_MaterialDebugPulseStartTime");

	public static int MaterialDebugLookupTexture = Shader.PropertyToID("_G_MaterialDebugLookupTexture");

	public static int MaterialDebugGlobalOverride = Shader.PropertyToID("_G_MaterialDebugGlobalOverride");
}
