public class TrainStationUnloaderEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane OutputLane;

	public TrainStationUnloaderEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
	}

	public bool TryAcceptFromTrain(BeltItem item)
	{
		if (OutputLane.HasItem)
		{
			return false;
		}
		OutputLane.Item = item;
		OutputLane.Progress_T = 0;
		OutputLane.MaxStep_S = 0;
		return true;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, OutputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, OutputLane);
	}
}
