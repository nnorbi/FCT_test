using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class AsteroidsDecorationJobified : IChunkedDecoration
{
	private readonly SpaceThemeBackgroundAsteroidsResources Resources;

	protected FastNoiseLite Noise;

	protected NativeArray<MeshMaterialID> AsteroidTypes;

	protected NativeArray<Bounds> MeshesBounds;

	protected NativeParallelMultiHashMap<int2, SpaceAsteroidInstance> ChunkAsteroidsCache;

	public bool ShouldDraw => Globals.Settings.Graphics.BackgroundDetails.Value >= GraphicsBackgroundDetails.High;

	public BackgroundChunkDecorationConfig BackgroundChunkDecorationConfig => new BackgroundChunkDecorationConfig
	{
		ChunkStartHeight_W = Resources.Generation_MinHeight,
		ChunkEndHeight_W = Resources.Generation_MaxHeight,
		ChunkBoundsPadding_W = 9000f,
		ChunkBoundsVerticalPadding_W = 5000f,
		ChunkSize_W = 50000f,
		MaxDrawnChunkCount = 140
	};

	public int MaxInstances => BackgroundChunkDecorationConfig.MaxDrawnChunkCount * Resources.Generation_GridSizeXY * 10 * 10 * Resources.Generation_GridSizeZ;

	public AsteroidsDecorationJobified(SpaceThemeBackgroundAsteroidsResources resources)
	{
		Resources = resources;
		Noise = new FastNoiseLite(resources.Seed);
		Noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		Noise.SetFrequency(0.02f);
		Noise.SetFractalOctaves(2);
	}

	public void RegisterResources(IBatchRenderer renderer)
	{
		ChunkAsteroidsCache = new NativeParallelMultiHashMap<int2, SpaceAsteroidInstance>(0, Allocator.Persistent);
		AsteroidTypes = new NativeArray<MeshMaterialID>(Resources.AsteroidMeshes.Length, Allocator.Persistent);
		MeshesBounds = new NativeArray<Bounds>(Resources.AsteroidMeshes.Length, Allocator.Persistent);
		BatchMaterialID materialId = renderer.RegisterMaterial(Resources.AsteroidMaterial);
		for (int i = 0; i < Resources.AsteroidMeshes.Length; i++)
		{
			Mesh mesh = Resources.AsteroidMeshes[i];
			AsteroidTypes[i] = new MeshMaterialID(renderer.RegisterMesh(mesh), materialId);
			MeshesBounds[i] = mesh.bounds;
		}
	}

	public void Dispose()
	{
		AsteroidTypes.Dispose();
		MeshesBounds.Dispose();
		ChunkAsteroidsCache.Dispose();
	}

	public void OnBeforeDraw(BatchRendererGroup brg)
	{
	}

	public void ScheduleDecorationDraw(NativeParallelHashSet<BackgroundChunkData> seenChunks, NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, float3 cameraPosition, NativeArray<Plane> cullingPlanes, float animationSimulationTime_G, ref JobHandle dependencies)
	{
		CollectAsteroidsJob collectAsteroidsJob = new CollectAsteroidsJob
		{
			CullingPlanes = cullingPlanes,
			BatchMeshes = AsteroidTypes,
			MeshesBounds = MeshesBounds,
			SeenChunks = seenChunks,
			BackgroundChunkDecorationConfig = BackgroundChunkDecorationConfig,
			Noise = Noise,
			SpaceChunkedDecorationNoiseParams = GetNoiseParams(),
			AsteroidMapping = ChunkAsteroidsCache,
			Instances = instances,
			SpinAnimationSpeed = Resources.SpinAnimationSpeed,
			AnimationSimulationTime_G = animationSimulationTime_G
		};
		dependencies = collectAsteroidsJob.Schedule(dependencies);
	}

	private SpaceChunkedDecorationNoiseParams GetNoiseParams()
	{
		return new SpaceChunkedDecorationNoiseParams
		{
			Seed = Resources.Seed,
			Generation_NoiseScaleXY = Resources.Generation_NoiseScaleXY,
			Generation_NoiseScaleZ = Resources.Generation_NoiseScaleZ,
			Generation_NoiseThreshold = Resources.Generation_NoiseThreshold,
			Generation_GridSizeXY = Resources.Generation_GridSizeXY,
			Generation_GridSizeZ = Resources.Generation_GridSizeZ,
			Generation_ScaleBase = Resources.Generation_ScaleBase,
			Generation_ScaleNoiseDependent = Resources.Generation_ScaleNoiseDependent,
			Generation_ScaleRandom = Resources.Generation_ScaleRandom,
			Generation_RandomOffset = Resources.Generation_RandomOffset
		};
	}
}
