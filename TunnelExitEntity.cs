public class TunnelExitEntity : MapEntity<MetaBuildingInternalVariant>
{
	public BeltLane[] Lanes = new BeltLane[4];

	public TunnelExitEntity(CtorArgs payload)
		: base(payload)
	{
		for (int i = 0; i < Lanes.Length; i++)
		{
			Lanes[i] = new BeltLane(InternalVariant.BeltLaneDefinitions[i]);
		}
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return Lanes[index];
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		for (int i = 0; i < Lanes.Length; i++)
		{
			BeltSimulation.UpdateLane(options, Lanes[i]);
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		for (int i = 0; i < Lanes.Length; i++)
		{
			BeltLane lane = Lanes[i];
			DrawDynamic_BeltLane(options, Lanes[i]);
		}
	}
}
