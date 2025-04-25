using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public interface IChunkedDecoration
{
	BackgroundChunkDecorationConfig BackgroundChunkDecorationConfig { get; }

	int MaxInstances { get; }

	bool ShouldDraw { get; }

	void RegisterResources(IBatchRenderer renderer);

	void ScheduleDecorationDraw(NativeParallelHashSet<BackgroundChunkData> seenChunks, NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, float3 cameraPosition, NativeArray<Plane> cullingPlanes, float animationSimulationTime_G, ref JobHandle dependencies);

	void Dispose();

	void OnBeforeDraw(BatchRendererGroup brg);
}
