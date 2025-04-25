#define ENABLE_PROFILER
using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public class FixedSharedBatchSystemsRenderer : IBatchRenderer
{
	private const int FLOAT4_SIZE = 16;

	private const int PACKED_MATRIX_SIZE = 48;

	private const int BYTES_PER_INSTANCE = 96;

	private const int INITIAL_PADDING = 96;

	private readonly IBatchRendererSystem[] Groups;

	private JobHandle CurrentJobHandle;

	private BatchID SharedBatchID;

	private readonly int MaxInstances;

	private readonly BatchRendererGroup BatchRendererGroup;

	private GraphicsBuffer InstanceData;

	private NativeArray<PackedMatrix> InstancesBuffer;

	private NativeRef<BatchCullingOutputDrawCommands> CullingOutputRef;

	private NativeMultiDictionary<MeshMaterialID, BatchInstance> InstanceMap;

	private RenderCategoryBucket FrameCategoryBucket;

	private static int BufferCountForInstances(int bytesPerInstance, int instances, int extraBytes)
	{
		bytesPerInstance = (bytesPerInstance + 4 - 1) / 4 * 4;
		extraBytes = (extraBytes + 4 - 1) / 4 * 4;
		int totalBytes = bytesPerInstance * instances + extraBytes;
		return totalBytes / 4;
	}

	public FixedSharedBatchSystemsRenderer(params IBatchRendererSystem[] groups)
	{
		Groups = groups;
		MaxInstances = groups.Sum((IBatchRendererSystem batchRendererSystem) => batchRendererSystem.MaxInstances);
		BatchRendererGroup = new BatchRendererGroup(OnPerformCulling, IntPtr.Zero);
		CullingOutputRef = new NativeRef<BatchCullingOutputDrawCommands>(Allocator.Persistent);
		InstanceMap = new NativeMultiDictionary<MeshMaterialID, BatchInstance>(0, Allocator.Persistent);
		CreateTargetInstanceData();
		foreach (IBatchRendererSystem group in groups)
		{
			group.RegisterResources(this);
		}
	}

	BatchMeshID IBatchRenderer.RegisterMesh(Mesh mesh)
	{
		return BatchRendererGroup.RegisterMesh(mesh);
	}

	BatchMaterialID IBatchRenderer.RegisterMaterial(Material material)
	{
		return BatchRendererGroup.RegisterMaterial(material);
	}

	private void CreateTargetInstanceData()
	{
		int bufferCount = BufferCountForInstances(96, MaxInstances, 96);
		InstancesBuffer = new NativeArray<PackedMatrix>((MaxInstances + 1) * 2, Allocator.Persistent);
		InstanceData = new GraphicsBuffer(GraphicsBuffer.Target.Raw, bufferCount, 4);
		InstanceData.SetData(InstancesBuffer);
		SharedBatchID = BatchRendererGroup.AddBatch(CreateMetadata(MaxInstances), InstanceData.bufferHandle);
	}

	private NativeArray<MetadataValue> CreateMetadata(int maxInstances)
	{
		int objectToWorldAddress = 96;
		int worldToObjectAddress = 96 + 48 * maxInstances;
		NativeArray<MetadataValue> metadata = new NativeArray<MetadataValue>(2, Allocator.Temp);
		metadata[0] = new MetadataValue
		{
			NameID = Shader.PropertyToID("unity_ObjectToWorld"),
			Value = (uint)(0x80000000u | objectToWorldAddress)
		};
		metadata[1] = new MetadataValue
		{
			NameID = Shader.PropertyToID("unity_WorldToObject"),
			Value = (uint)(0x80000000u | worldToObjectAddress)
		};
		return metadata;
	}

	public void ScheduleDraw(FrameDrawOptions drawOptions)
	{
		CurrentJobHandle.Complete();
		FrameCategoryBucket = drawOptions.RenderStats.GetBucket(RenderCategory.BurstBackground);
		if (FrameCategoryBucket.RenderingEnabled)
		{
			Profiler.BeginSample("Schedule Draw (Main Thread)");
			InstanceMap.Clear();
			CullingOutputRef.Value = default(BatchCullingOutputDrawCommands);
			IBatchRendererSystem[] groups = Groups;
			foreach (IBatchRendererSystem group in groups)
			{
				group.OnBeforeDraw(BatchRendererGroup);
			}
			JobHandle handle = default(JobHandle);
			IBatchRendererSystem[] groups2 = Groups;
			foreach (IBatchRendererSystem group2 in groups2)
			{
				group2.ScheduleGroupDraw(drawOptions, in InstanceMap, ref handle);
			}
			CreateDrawCallsJob createDrawCommandsJobs = new CreateDrawCallsJob
			{
				Instances = InstanceMap,
				CullingOutput = CullingOutputRef,
				PackedMatrices = InstancesBuffer,
				SharedBatchID = SharedBatchID,
				MaxInstances = MaxInstances
			};
			handle = createDrawCommandsJobs.Schedule(handle);
			JobHandle.ScheduleBatchedJobs();
			CurrentJobHandle = handle;
			Profiler.EndSample();
		}
	}

	private JobHandle OnPerformCulling(BatchRendererGroup rendererGroup, BatchCullingContext cullingContext, BatchCullingOutput cullingOutput, IntPtr userContext)
	{
		if (cullingContext.viewType != BatchCullingViewType.Camera)
		{
			return default(JobHandle);
		}
		if ((cullingContext.cullingLayerMask & 1) == 0)
		{
			return default(JobHandle);
		}
		CurrentJobHandle.Complete();
		int visible = CullingOutputRef.Value.visibleInstanceCount;
		if (visible == 0)
		{
			return default(JobHandle);
		}
		cullingOutput.drawCommands[0] = CullingOutputRef.Value;
		CullingOutputRef.Value = default(BatchCullingOutputDrawCommands);
		Profiler.BeginSample("Update instances matrix");
		InstanceData.SetData(InstancesBuffer, 0, 0, visible);
		Profiler.EndSample();
		return default(JobHandle);
	}

	private unsafe void TrackDrawCalls(in BatchCullingOutputDrawCommands drawCommands)
	{
		for (int i = 0; i < drawCommands.drawCommandCount; i++)
		{
			BatchDrawCommand drawCall = drawCommands.drawCommands[i];
			Mesh mesh = BatchRendererGroup.GetRegisteredMesh(drawCall.meshID);
			Material material = BatchRendererGroup.GetRegisteredMaterial(drawCall.materialID);
			if (mesh == null)
			{
				if (Application.platform != RuntimePlatform.OSXEditor)
				{
					throw new Exception("Mesh is null");
				}
				continue;
			}
			int triangles = (int)mesh.GetIndexCount(0) / 3;
			if (material.renderQueue <= 2500)
			{
				triangles *= 2;
			}
		}
	}

	public void Dispose()
	{
		CurrentJobHandle.Complete();
		InstancesBuffer.Dispose();
		InstanceData.Dispose();
		InstanceMap.Dispose();
		CullingOutputRef.Dispose();
		BatchRendererGroup.Dispose();
		IBatchRendererSystem[] groups = Groups;
		foreach (IBatchRendererSystem group in groups)
		{
			group.Dispose();
		}
	}
}
