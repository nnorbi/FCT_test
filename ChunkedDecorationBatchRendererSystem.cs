using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class ChunkedDecorationBatchRendererSystem : IBatchRendererSystem
{
	private readonly IChunkedDecoration Decoration;

	private float AnimationSimulationTime_G;

	public int MaxInstances => Decoration.MaxInstances;

	public ChunkedDecorationBatchRendererSystem(IChunkedDecoration decoration)
	{
		Decoration = decoration;
	}

	public void RegisterResources(IBatchRenderer batchRenderer)
	{
		Decoration.RegisterResources(batchRenderer);
	}

	public void OnBeforeDraw(BatchRendererGroup brg)
	{
		Decoration.OnBeforeDraw(brg);
	}

	public void ScheduleGroupDraw(FrameDrawOptions options, in NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, ref JobHandle dependencies)
	{
		if (Decoration.ShouldDraw)
		{
			float2 screenCenter = new float2((float)Screen.width / 2f, (float)Screen.height / 2f);
			if (ScreenUtils.TryGetTileCoordinate(options.Player.Viewport, (Decoration.BackgroundChunkDecorationConfig.ChunkStartHeight_W + Decoration.BackgroundChunkDecorationConfig.ChunkEndHeight_W) / 2f, in screenCenter, out var screenCenterPos_G))
			{
				NativeArray<Plane> cullingPlanes = options.CameraPlanes.ToNativeArray(Allocator.TempJob);
				AnimationSimulationTime_G = options.AnimationSimulationTime_G;
				NativeParallelHashSet<BackgroundChunkData> chunks = new NativeParallelHashSet<BackgroundChunkData>(Decoration.BackgroundChunkDecorationConfig.MaxDrawnChunkCount, Allocator.TempJob);
				CollectChunksJob collectChunksJob = new CollectChunksJob
				{
					ChunksConfig = Decoration.BackgroundChunkDecorationConfig,
					Chunks = chunks,
					ScreenCenterPos_G = screenCenterPos_G,
					CameraPlanes = cullingPlanes
				};
				dependencies = collectChunksJob.Schedule(dependencies);
				Decoration.ScheduleDecorationDraw(chunks, instances, options.CameraPosition_W, cullingPlanes, AnimationSimulationTime_G, ref dependencies);
				chunks.Dispose(dependencies);
				cullingPlanes.Dispose(dependencies);
			}
		}
	}

	public void Dispose()
	{
		Decoration.Dispose();
	}

	void IBatchRendererSystem.ScheduleGroupDraw(FrameDrawOptions drawOptions, in NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, ref JobHandle dependencies)
	{
		ScheduleGroupDraw(drawOptions, in instances, ref dependencies);
	}
}
