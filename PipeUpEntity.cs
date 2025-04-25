using Unity.Mathematics;

public class PipeUpEntity : PipeEntity
{
	protected float GearRotation = 0f;

	public PipeUpEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		if (Container.ConnectedContainers.Count > 0)
		{
			float delta = Container.GetSignedFlowRateAtIndex(0);
			GearRotation += delta * 360f * options.DeltaTime;
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[^1], new float3(0f, 0f, 0f), GearRotation);
		Draw_PipeFluidMesh(options, InternalVariant.SupportMeshesInternalLOD.Length - 1);
	}
}
