public class TrainStationLoaderEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane FakeProcessingLane;

	public TrainStationLoaderEntity(CtorArgs payload)
		: base(payload)
	{
		FakeProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], FakeProcessingLane);
	}

	public BeltItem TryUnloadToTrain()
	{
		if (!FakeProcessingLane.HasItem)
		{
			return null;
		}
		BeltItem result = FakeProcessingLane.Item;
		FakeProcessingLane.ClearLane();
		return result;
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, FakeProcessingLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, FakeProcessingLane);
	}
}
