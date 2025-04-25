using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "SpaceThemeResources", menuName = "Metadata/Themes/Space/Resources")]
public class SpaceThemeResources : ScriptableObject
{
	[Header("Linked Resources")]
	public SpaceThemeIslandResources Islands;

	[Header("Background - Cylinders")]
	[Space(20f)]
	public SpaceThemeBackgroundCylinders.ExtraResources BackgroundCylinders;

	[Header("Background - Asteroids")]
	[Space(20f)]
	public SpaceThemeBackgroundAsteroidsResources BackgroundAsteroids;

	[Header("Background - Dark Matter Streams")]
	[Space(20f)]
	public SpaceThemeBackgroundDarkMatter.ExtraResources BackgroundDarkMatter;

	[Header("Background - Floating Shapes")]
	[Space(20f)]
	public SpaceThemeBackgroundFloatingShapes.ExtraResources BackgroundFloatingShapes;

	[Header("Background - Stars")]
	[Space(20f)]
	public SpaceThemeBackgroundStarsResources BackgroundStars;

	[Header("Background - Skybox Stars")]
	[Space(20f)]
	public SpaceThemeBackgroundSkyboxStars.ExtraResources BackgroundSkyboxStars;

	[Header("Background - Particle Clouds")]
	[Space(20f)]
	public SpaceThemeBackgroundParticleCloudResources BackgroundParticleClouds;

	[Header("Background - Comets")]
	[Space(20f)]
	public SpaceThemeBackgroundComets.ExtraResources BackgroundComets;

	[Header("Background - Nebulas")]
	[Space(20f)]
	public SpaceThemeBackgroundNebulas.ExtraResources BackgroundNebulas;

	[Header("Background - Skybox")]
	[Space(20f)]
	public SpaceThemeBackgroundSkybox.ExtraResources BackgroundSkybox;

	public float ShapeAsteroidHeight = -45f;

	public float ShapeAsteroidShapeOffset = 12.5f;

	public float3 ShapeAsteroidShapeScale = new float3(30f, 120f, 30f);

	[Header("Fluid Asteroids")]
	[Space(10f)]
	public LOD2Mesh FluidAsteroidMainMesh;

	public LOD2Mesh[] FluidAsteroidDecorationMeshes;

	[Min(0f)]
	public float FluidAsteroidDecorationMaxDistance = 2000f;

	[Min(0f)]
	public float FluidAsteroidCubesMaxDistance = 1000f;

	[Min(0f)]
	public float FluidAsteroidLOD1Distance = 1000f;

	public LOD2Mesh FluidAsteroidInternalCubes;

	public float FluidAsteroidScale;

	public Material FluidCloudBackFacesDepthPassMaterial;

	public Material FluidCloudFrontFacesDepthPassMaterial;

	public Material FluidCloudsInternalCubesMaterial;

	public float FluidCloudHeightVariation = 0.5f;

	public float FluidCloudHeight = -32f;

	[Header("Island Tunnels")]
	[Space(30f)]
	[ValidateMesh(1000)]
	public Mesh IslandTunnelsConnectionMesh;

	[ValidateMesh(1000)]
	public Mesh IslandTunnelsConnectionMeshGlass;

	[ValidateMesh(1000)]
	public Mesh IslandTunnelsEntranceArrowMesh;

	public Material IslandTunnelsEntranceArrowMaterial;

	[ValidateMesh(1000)]
	public Mesh IslandTunnelsExitArrowMesh;

	public Material IslandTunnelsExitArrowMaterial;

	public Material IslandTunnelsGlassMaterial;

	[Header("Ground Fog")]
	[Space(10f)]
	public Material GroundFogMaterial;

	public float GroundFogOffset = -0.5f;

	public float GroundFogParticleScale = 5f;

	public float GroundFogParticleDistance = -0.01f;

	[Header("Shape Miner")]
	public Material ShapeMinerLaserParticleMaterial;

	public Material ShapeMinerLaserMaterial;

	[ValidateMesh(500)]
	public Mesh ShapeMinerLaserMesh;

	public EditorDict<MetaShapeColor, Material> FluidCloudMaterials;

	[Header("Shape Asteroids")]
	[Space(30f)]
	public ShapeAsteroidVisualization ShapeAsteroidVisualization;
}
