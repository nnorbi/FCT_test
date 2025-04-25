using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;

[BurstCompile]
internal struct CreateDrawCallsJob : IJob
{
	[ReadOnly]
	public NativeMultiDictionary<MeshMaterialID, BatchInstance> Instances;

	[WriteOnly]
	public NativeArray<PackedMatrix> PackedMatrices;

	public NativeRef<BatchCullingOutputDrawCommands> CullingOutput;

	public int MaxInstances;

	public BatchID SharedBatchID;

	public unsafe void Execute()
	{
		BatchCullingOutputDrawCommands drawCommand = default(BatchCullingOutputDrawCommands);
		if (Instances.UniqueKeysCount() == 0)
		{
			return;
		}
		NativeArray<MeshMaterialID> drawCalls = Instances.UniqueKeys.ToNativeArray(Allocator.Temp);
		drawCommand.drawCommandCount = drawCalls.Length;
		drawCommand.drawCommands = UnsafeExtensions.Malloc<BatchDrawCommand>(drawCalls.Length);
		drawCommand.drawRangeCount = 1;
		drawCommand.drawRanges = UnsafeExtensions.Malloc<BatchDrawRange>(1);
		drawCommand.drawCommandPickingInstanceIDs = null;
		drawCommand.instanceSortingPositions = null;
		drawCommand.instanceSortingPositionFloatCount = 0;
		int packedMatricesIndex = 2;
		int packedInverseMatricesIndex = MaxInstances + 2;
		int totalInstances = 0;
		for (int drawCallIndex = 0; drawCallIndex < drawCalls.Length; drawCallIndex++)
		{
			NativeList<BatchInstance> instances = Instances.GetValueListForKey(drawCalls[drawCallIndex], Allocator.Temp);
			int drawInstances = 0;
			foreach (BatchInstance instance in instances)
			{
				PackedMatrices[packedMatricesIndex++] = new PackedMatrix(instance.LocalToWorld);
				PackedMatrices[packedInverseMatricesIndex++] = new PackedMatrix(math.fastinverse(instance.LocalToWorld));
				drawInstances++;
			}
			instances.Dispose();
			BatchDrawCommand drawCall = new BatchDrawCommand
			{
				visibleOffset = (uint)totalInstances,
				visibleCount = (uint)drawInstances,
				batchID = SharedBatchID,
				meshID = drawCalls[drawCallIndex].MeshID,
				materialID = drawCalls[drawCallIndex].MaterialID,
				submeshIndex = 0,
				splitVisibilityMask = 255,
				flags = BatchDrawCommandFlags.None,
				sortingPosition = 0
			};
			totalInstances += drawInstances;
			drawCommand.drawCommands[drawCallIndex] = drawCall;
		}
		drawCalls.Dispose();
		drawCommand.drawRanges->drawCommandsBegin = 0u;
		drawCommand.drawRanges->drawCommandsCount = (uint)drawCalls.Length;
		drawCommand.drawRanges->filterSettings = new BatchFilterSettings
		{
			renderingLayerMask = uint.MaxValue
		};
		drawCommand.visibleInstanceCount = totalInstances;
		drawCommand.visibleInstances = UnsafeExtensions.Malloc<int>(totalInstances);
		for (int i = 0; i < totalInstances; i++)
		{
			drawCommand.visibleInstances[i] = i;
		}
		CullingOutput.Value = drawCommand;
	}
}
