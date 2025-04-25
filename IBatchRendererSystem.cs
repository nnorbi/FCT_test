using Unity.Jobs;
using UnityEngine.Rendering;

public interface IBatchRendererSystem
{
	int MaxInstances { get; }

	void RegisterResources(IBatchRenderer batchRenderer);

	void OnBeforeDraw(BatchRendererGroup brg);

	void ScheduleGroupDraw(FrameDrawOptions drawOptions, in NativeMultiDictionary<MeshMaterialID, BatchInstance> instances, ref JobHandle dependencies);

	void Dispose();
}
