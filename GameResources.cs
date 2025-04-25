using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameResources", menuName = "Metadata/GameResources")]
public class GameResources : ScriptableObject
{
	[Header("General Configuration")]
	public float BeltShapeHeight = 0.2f;

	public float ShapeInnerGap = 0.0125f;

	public float ShapeDimensions2D = 0.3f;

	public float ShapeLayerScaleReduction = 0.15f;

	public float ShapeLayerHeight = 0.04f;

	public float ShapeSupportHeight = 0.02f;

	public int DistanceBetweenStands = 5;

	public float[] LODDistancesBuildings = new float[4] { 30f, 60f, 120f, 240f };

	public float[] LODDistancesIsland = new float[4] { 60f, 120f, 240f, 600f };

	[Header("Game Modes")]
	[Space(20f)]
	[SerializeField]
	private MetaGameMode[] GameModes;

	[Header("Layers")]
	[Space(20f)]
	public Color[] LayerColors;

	public Color[] BeltArrowColors;

	[Header("Theme Colors")]
	[Space(20f)]
	public EditorShaderColor ThemePrimary;

	public EditorShaderColor ThemeErrorOrDelete;

	public EditorShaderColor ThemeWarning;

	public EditorShaderColor ThemeNeutral;

	[Header("Placement Colors")]
	[Space(20f)]
	public EditorShaderColor ThemeWillBePlaced;

	public EditorShaderColor ThemeWillBePlacedWithWarning;

	public EditorShaderColor ThemeWontBePlaced;

	public EditorShaderColor ThemeImpossibleToPlace;

	[Header("Materials")]
	[Space(20f)]
	public Material DefaultUISpriteMaterial;

	public Material ShapeMaterial;

	public Material ShapeMaterialUIPrerender;

	public Material ShapeMaterialUI;

	public Material ShapeMaterialFadeout;

	public Material ShapeMaterialDissolve;

	public Material TranslucentDefaultMaterial;

	[RequiredListLength(MinLength = 5, MaxLength = 5)]
	public Material[] DebugLODMaterials;

	public Material DebugIndicatorMeshCombinedShader;

	public Material DebugIndicatorMeshNotCombinedShader;

	[Header("Meshes - General")]
	[Space(20f)]
	[ValidateMesh]
	public Mesh ShapeSupportMesh;

	[ValidateMesh]
	public Mesh ShapeLeftSupportMesh;

	[ValidateMesh]
	public Mesh ShapeRightSupportMesh;

	[ValidateMesh]
	public Mesh FluidPatchMesh;

	[ValidateMesh]
	public Mesh UXPlaneMeshUVMapped;

	[ValidateMesh]
	public Mesh ShapeCrateMesh;

	[Space(20f)]
	[Header("Placement Animations")]
	public AnimationCurve PlacementAnimationScaleFactor = new AnimationCurve();

	public AnimationCurve PlacementAnimationAlpha = new AnimationCurve();

	public AnimationCurve DeletionAnimationScaleFactor = new AnimationCurve();

	public AnimationCurve DeletionAnimationAlpha = new AnimationCurve();

	public Material BuildingPlaceAnimationMaterial;

	[Header("Selection")]
	[Space(20f)]
	[ValidateMesh]
	public Mesh AreaSelectCornerBottom;

	[ValidateMesh]
	public Mesh AreaSelectEdgeX;

	[ValidateMesh]
	public Mesh AreaSelectEdgeY;

	[ValidateMesh]
	public Mesh AreaSelectEdgeZ;

	[Header("UI / Island Placement & Crafting")]
	public Sprite UIIslandChunkPreview;

	public EditorDict<string, Sprite> UIGlobalIconMapping;

	[Header("HUB")]
	[Space(20f)]
	public HubEntity.AnimationParameters HUBAnimationParameters;

	public IEnumerable<MetaGameMode> SupportedGameModes
	{
		get
		{
			MetaGameMode[] gameModes = GameModes;
			foreach (MetaGameMode mode in gameModes)
			{
				if (mode.AvailableInDemo || !GameEnvironmentManager.IS_DEMO)
				{
					yield return mode;
				}
			}
		}
	}
}
