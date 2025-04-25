using System;
using Unity.Mathematics;

[Serializable]
public class Splitter1To2Entity : SplitterEntity
{
	public Splitter1To2Entity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		for (int i = 0; i < OutputLanes.Length; i++)
		{
			DrawDynamic_BeltLane(options, OutputLanes[i]);
		}
		DrawDynamic_BeltLane(options, InputLane);
		BeltLane outputLane = OutputLanes[1];
		float pusherProgress = 0f;
		if (outputLane.HasItem)
		{
			float laneY = outputLane.Definition.GetPosFromTicks_L(outputLane.Progress_T).y;
			pusherProgress = laneY * InternalVariant.GetCurve(0, outputLane.Progress);
		}
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0f, pusherProgress, 0f));
	}
}
