using System;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

[Unity.Jobs.DOTSCompilerGenerated]
internal class __JobReflectionRegistrationOutput__1315227775
{
	public static void CreateJobReflectionData()
	{
		try
		{
			IJobExtensions.EarlyJobInit<CollectAsteroidsJob>();
			IJobExtensions.EarlyJobInit<CollectChunksJob>();
			IJobExtensions.EarlyJobInit<CollectParticleCloudsJob>();
			IJobExtensions.EarlyJobInit<CollectStarsJob>();
			IJobExtensions.EarlyJobInit<CreateDrawCallsJob>();
			IJobExtensions.EarlyJobInit<BoundsAggregationJob>();
			IJobParallelForExtensions.EarlyJobInit<LazyMeshCombinerJob<PosNormalTangent4UV0, BatchMeshID>>();
		}
		catch (Exception ex)
		{
			EarlyInitHelpers.JobReflectionDataCreationFailed(ex);
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	public static void EarlyInit()
	{
		CreateJobReflectionData();
	}
}
