using JetBrains.Annotations;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class ParticleCloudsDecorationJobified : IChunkedDecoration
{
	private readonly SpaceThemeBackgroundParticleCloudResources Resources;

	protected FastNoiseLite Noise;

	protected NativeParallelMultiHashMap<int2, ParticleCloudInstance> ChunkParticleCloudsCache;

	private BatchMeshID GridMeshId;

	protected NativeArray<BatchMaterialID> ParticleMaterialsIds;

	[UsedImplicitly]
	private Mesh GridMesh;

	public bool ShouldDraw => Globals.Settings.Graphics.BackgroundDetails.Value >= GraphicsBackgroundDetails.High;

	public BackgroundChunkDecorationConfig BackgroundChunkDecorationConfig => new BackgroundChunkDecorationConfig
	{
		ChunkStartHeight_W = Resources.Generation_MinHeight,
		ChunkEndHeight_W = Resources.Generation_MaxHeight,
		ChunkBoundsPadding_W = 2500f,
		ChunkBoundsVerticalPadding_W = 2500f,
		ChunkSize_W = 50000f,
		MaxDrawnChunkCount = 100
	};

	public int MaxInstances => BackgroundChunkDecorationConfig.MaxDrawnChunkCount * Resources.Generation_GridSizeXY * 10 * 10 * Resources.Generation_GridSizeZ;

	public ParticleCloudsDecorationJobified(SpaceThemeBackgroundParticleCloudResources resources)
	{
		Resources = resources;
		Noise = new FastNoiseLite(893);
		Noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		Noise.SetFrequency(0.02f);
		Noise.SetFractalOctaves(2);
	}

	public void RegisterResources(IBatchRenderer renderer)
	{
		Mesh gridMesh = GeometryHelpers.GenerateTransformedMesh_UNCACHED(GeometryHelpers.MakePlaneMeshUV_UNCACHED(default(Color)), Matrix4x4.Rotate(Quaternion.Euler(-90f, 0f, 0f)));
		gridMesh.name = "SpaceThemeBackgroundParticleCloud";
		GridMesh = gridMesh;
		GridMeshId = renderer.RegisterMesh(gridMesh);
		ChunkParticleCloudsCache = new NativeParallelMultiHashMap<int2, ParticleCloudInstance>(0, Allocator.Persistent);
		ParticleMaterialsIds = new NativeArray<BatchMaterialID>(Resources.ParticleMaterial.Length, Allocator.Persistent);
		for (int i = 0; i < Resources.ParticleMaterial.Length; i++)
		{
			ParticleMaterialsIds[i] = renderer.RegisterMaterial(Resources.ParticleMaterial[i]);
		}
	}

	public void Dispose()
	{
		ParticleMaterialsIds.Dispose();
		ChunkParticleCloudsCache.Dispose();
	}

	public void OnBeforeDraw(BatchRendererGroup brg)
	{
	}

	public void ScheduleDecorationDraw(NativeParallelHashSet<BackgroundChunkData> seenChunks, NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, float3 cameraPosition, NativeArray<Plane> cullingPlanes, float animationSimulationTime_G, ref JobHandle dependencies)
	{
		CollectParticleCloudsJob collectParticleCloudsJob = new CollectParticleCloudsJob
		{
			CullingPlanes = cullingPlanes,
			BatchMaterials = ParticleMaterialsIds,
			SeenChunks = seenChunks,
			BackgroundChunkDecorationConfig = BackgroundChunkDecorationConfig,
			Noise = Noise,
			SpaceChunkedDecorationNoiseParams = GetNoiseParams(),
			ParticleCloudMapping = ChunkParticleCloudsCache,
			MeshID = GridMeshId,
			CameraPosition = cameraPosition,
			Instances = instances
		};
		dependencies = collectParticleCloudsJob.Schedule(dependencies);
	}

	private SpaceChunkedDecorationNoiseParams GetNoiseParams()
	{
		return new SpaceChunkedDecorationNoiseParams
		{
			Seed = Resources.Generation_Seed,
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
