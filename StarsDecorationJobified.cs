using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class StarsDecorationJobified : IChunkedDecoration
{
	private readonly SpaceThemeBackgroundStarsResources Resources;

	protected NativeParallelHashMap<ChunkMaterialTuple, LazyMeshId> LazyCombinedMeshPerChunkPerMaterial;

	private BatchMeshID MeshID;

	private NativeArray<BatchMaterialID> MaterialIDs;

	protected UnmanagedMeshCombiner UnmanagedMeshCombiner;

	private NativeHashSet<BackgroundChunkData> InitializedChunks;

	public bool ShouldDraw => Globals.Settings.Graphics.BackgroundDetails.Value >= GraphicsBackgroundDetails.Medium;

	public int MaxInstances => BackgroundChunkDecorationConfig.MaxDrawnChunkCount * Resources.CountPerChunk * 81 * Resources.StarMaterial.Length;

	public BackgroundChunkDecorationConfig BackgroundChunkDecorationConfig => new BackgroundChunkDecorationConfig
	{
		ChunkStartHeight_W = Resources.Generation_MinHeight,
		ChunkEndHeight_W = Resources.Generation_MaxHeight,
		ChunkBoundsPadding_W = 10f,
		ChunkBoundsVerticalPadding_W = 10f,
		ChunkSize_W = 50000f,
		MaxDrawnChunkCount = 100
	};

	public StarsDecorationJobified(SpaceThemeBackgroundStarsResources resources)
	{
		Resources = resources;
	}

	public void RegisterResources(IBatchRenderer renderer)
	{
		MeshID = renderer.RegisterMesh(Resources.StarMesh);
		MaterialIDs = new NativeArray<BatchMaterialID>(Resources.StarMaterial.Length, Allocator.Persistent);
		for (int i = 0; i < Resources.StarMaterial.Length; i++)
		{
			MaterialIDs[i] = renderer.RegisterMaterial(Resources.StarMaterial[i]);
		}
		LazyCombinedMeshPerChunkPerMaterial = new NativeParallelHashMap<ChunkMaterialTuple, LazyMeshId>(0, Allocator.Persistent);
		InitializedChunks = new NativeHashSet<BackgroundChunkData>(0, Allocator.Persistent);
		UnmanagedMeshCombiner = LazyMeshCombinationManager.UnmanagedMeshCombiner;
	}

	public void ScheduleDecorationDraw(NativeParallelHashSet<BackgroundChunkData> seenChunks, NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, float3 cameraPosition, NativeArray<Plane> cullingPlanes, float animationSimulationTime_G, ref JobHandle dependencies)
	{
		CollectStarsJob collectStarsJob = new CollectStarsJob
		{
			SeenChunks = seenChunks,
			Chunks = BackgroundChunkDecorationConfig,
			LazyCombinedMeshPerChunkPerMaterial = LazyCombinedMeshPerChunkPerMaterial,
			UnmanagedMeshCombiner = UnmanagedMeshCombiner,
			InitializedChunks = InitializedChunks,
			Instances = instances,
			MeshID = MeshID,
			MaterialIDs = MaterialIDs,
			GenerationSeed = Resources.Generation_Seed,
			CountPerChunk = Resources.CountPerChunk,
			GenerationScaleBase = Resources.Generation_ScaleBase,
			GenerationScaleRandom = Resources.Generation_ScaleRandom,
			GenerationScaleDepthDecay = Resources.Generation_ScaleDepthDecay
		};
		dependencies = collectStarsJob.Schedule(dependencies);
	}

	public void Dispose()
	{
		LazyCombinedMeshPerChunkPerMaterial.Dispose();
		InitializedChunks.Dispose();
		MaterialIDs.Dispose();
	}

	public void OnBeforeDraw(BatchRendererGroup brg)
	{
		LazyMeshCombinationManager.Update(brg);
	}
}
