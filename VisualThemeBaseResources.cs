using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "VisualThemeBaseResources", menuName = "Metadata/Themes/Base Resources")]
public class VisualThemeBaseResources : ScriptableObject
{
	[Header("Colors")]
	public Color PlayingfieldOverviewColor = new Color(0.7f, 0.7f, 0.7f);

	[Header("Materials")]
	public Material BuildingsGlassMaterial;

	public Material BuildingMaterial;

	public Material BuildingsOverviewMaterial;

	public Material IslandFramesMaterial;

	public Material BuildingLaserMaterial;

	public Material OpaqueBuildingLaserMaterial;

	public Material BlueprintMaterial;

	public Material BlueprintPreWriteDepthMaterial;

	[Header("Playing Field")]
	[ValidateMesh]
	[RequiredListLength(16, null)]
	public Mesh[] TileMeshes;

	[ValidateMesh]
	public Mesh TileCornerFillMesh;

	public Material PlayingfieldMaterial;

	[RequiredListLength(2, null)]
	public Material[] PlayingfieldCurrentLayerPlaneMaterialPerLayer;

	[Header("Notches")]
	public LOD4Mesh NotchMesh;

	[Header("Rails - DEBUG")]
	public float RailHeight = -5f;

	public Material TrainMaterial;

	[ValidateMesh(5000)]
	public Mesh TrainMesh;

	[Header("HUB")]
	public Material HUBGlowMaterial;

	[Header("Patches")]
	public LOD3Mesh ShapePatchTubeClosedMesh;

	public Material ShapePatchLidMaterial;

	public LOD3Mesh ShapePatchTileGlowMesh;

	public Material ShapePatchTileGlowMaterial;

	[Header("Fluids")]
	public EditorDict<MetaShapeColor, Material> FluidMaterials;

	[Header("Pipes")]
	[Space(20f)]
	[RequiredListLength(1, null)]
	public LOD4Mesh[] PipeStandsAndEndCap;

	[Space(20f)]
	[RequiredListLength(1, null)]
	public LOD4Mesh[] PipeStandsBetweenPipes;

	[Space(20f)]
	[RequiredListLength(1, null)]
	public LOD4Mesh[] PipeBuildingStandsAndEndCap;

	[Space(20f)]
	public LOD4Mesh PipeBuildingConnector;

	[Header("Meshes - Belt Caps & Stands")]
	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapInput;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapInputWithBorder;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapInputConflict;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapInputBorderOnly;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapOutput;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapOutputWithBorder;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapOutputBorderOnly;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD3Mesh[] BeltCapOutputConflict;

	[Space(20f)]
	[RequiredListLength(3, null)]
	public LOD4Mesh[] BeltCapStandsNormal;

	[Space(20f)]
	public LOD3Mesh BuildingSeperators;

	public LOD3Mesh BuildingSeperatorsShared;

	[Header("UI/UX")]
	public Material UXShapeHollowMaterial;

	public Material UXExtractorPlacementValidMaterial;

	public Material UXHubSpotPlacementValidMaterial;

	public Material UXHubSpotLockedMaterial;

	public Material UXIslandSelectorMaterial;

	public Material UXIslandHoverMaterial;

	public Material UXIslandBlueprintMaterial;

	public Material UXIslandBlueprintInvalidMaterial;

	public Material UXBuildingBeltInputConnectedMaterial;

	public Material UXBuildingBeltInputNotConnectedMaterial;

	public Material UXBuildingBeltOutputConnectedMaterial;

	public Material UXBuildingBeltOutputNotConnectedMaterial;

	public Material UXBuildingBeltIOConflictMaterial;

	public Material UXBuildingFluidIOConnectedMaterial;

	public Material UXBuildingFluidIONotConnectedMaterial;

	public Material UXBeltPathCheckpointMaterial;

	public Material UXHyperBeltPathCheckpointMaterial;

	public Material UXMinerIslandResourceIndicatorMaterial;

	public Material UXHubSpotIndicatorUnusedMaterial;

	public Material UXHubSpotIndicatorInvalidMaterial;

	public Material UXHubSpotIndicatorValidMaterial;

	public Material UXHubSpotIndicatorOutdatedMaterial;

	public Material UXRailForwardMaterial;

	public Material UXRailRightMaterial;

	public Material UXRailLeftMaterial;

	public Material UXRailPlacementForwardMaterial;

	public Material UXRailPlacementRightMaterial;

	public Material UXRailPlacementLeftMaterial;

	public Material UXRailPlacementOverviewPlaneMaterial;

	public Material UXHalfCutterPlacementIndicatorMaterial;

	public Material UXBuildingAreaSelectionIndicatorMaterial;

	public Material UXIslandAreaSelectionIndicatorMaterial;

	public Material UXBuildingSelectionMaterial;

	public Material UXGeneralBuildingPlacementIndicatorMaterial;

	[Space(20f)]
	[Header("Overview Mode")]
	public Material UXOverviewModeBackgroundPlaneMaterial;

	public Material UXOverviewModeFluidPlaneMaterial;

	public Material UXOverviewModeShapePlaneMaterial;

	public Material UXOverviewModeIslandChunkMaterial;

	public Material UXOverviewModeRailForwardMaterial;

	public Material UXOverviewModeRailLeftMaterial;

	public Material UXOverviewModeRailRightMaterial;

	public Material UXOverviewModeTrain;

	public Material UXOverviewModeTunnelsMaterial;

	public Material UXOverviewModeExploredAreaIsland;

	public Material UXOverviewModeExploredAreaSuperChunk;

	public Material UXOverviewModeExploredAreaSuperChunkHUB;

	[ValidateMesh(1000)]
	public Mesh UXOverviewModeIslandChunkMesh;

	[Space(20f)]
	[Header("Notches")]
	public Material UXNotchPlayingfieldMaterial;

	public Material UXNotchActivePlayingfieldMaterial;

	public Material UXNotchActiveBeltSenderMaterial;

	public Material UXNotchActiveBeltReceiverMaterial;

	public Material UXNotchActiveTrainLoaderMaterial;

	public Material UXNotchActiveTrainUnloaderMaterial;

	public Material UXNotchActiveMixedMaterial;

	public Material UXBeltPortPlacementReceiverMaterial;

	public Material UXBeltPortPlacementSenderMaterial;

	public Material UXBeltPortPlacementPathParticleMaterial;

	[Space(20f)]
	[Header("Visualizations")]
	public Material UXShapeResourceVisualizationUnderlayMaterial;

	public Material UXTrainLinesVisualizationTrainMaterial;

	public Material UXTrainStationsVisualizationLoaderMaterial;

	public Material UXTrainStationsVisualizationUnloaderMaterial;

	public Material UXIslandShapeTransferVisualizationSenderMaterial;

	public Material UXIslandShapeTransferVisualizationReceiverMaterial;

	public Material UXIslandShapeTransferVisualizationShapeUnderlayMaterial;

	public Material UXTunnelsVisualizationConnectorMaterial;

	public Material UXIslandGridVisualizationGridMaterial;

	public Material UXBeltPathIslandConnectionStartComplete;

	public Material UXBeltPathIslandConnectionMiddleComplete;

	public Material UXBeltPathIslandConnectionEndComplete;

	public Material UXBeltPathIslandConnectionStartIncomplete;

	public Material UXBeltPathIslandConnectionMiddleIncomplete;

	public Material UXIslandGridVisualizationLowerGroundMaterial;

	public Material UXBuildingHoverIndicatorMaterial;

	public Material UXShapePredictionMaterial;

	public Material UXShapeOutputPredictionActiveIndicatorMaterial;

	public Material UXShapeOutputPredictionInactiveIndicatorMaterial;

	public Material UXShapePredictionLoadingMaterial;

	public Material UXShapePredictionNullMaterial;

	public void ProvideShaderInputs()
	{
		Shader.SetGlobalColor(GlobalShaderInputs.IslandOverviewPlayingfieldColor, PlayingfieldOverviewColor);
	}
}
