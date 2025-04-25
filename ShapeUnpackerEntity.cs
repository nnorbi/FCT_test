using Unity.Mathematics;

public class ShapeUnpackerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane LowerOutputLane;

	protected BeltLane UpperOutputLane;

	protected ShapeDefinition CurrentItem;

	protected int CurrentItemRemaining;

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1]
		{
			new HUDSidePanelModuleStatShapeCrateSize(ShapeCrateItem.SIZE)
		};
	}

	public ShapeUnpackerEntity(CtorArgs payload)
		: base(payload)
	{
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0]);
		LowerOutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		UpperOutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2]);
		InputLane.PreAcceptHook = (BeltItem item) => (CurrentItem == null) ? item : null;
		InputLane.PostAcceptHook = ItemPostAcceptHook;
		InputLane.MaxStep_S = InputLane.Definition.Length_S;
	}

	protected void ItemPostAcceptHook(BeltLane lane, ref int remainingTicks_T)
	{
		CurrentItem = ((ShapeCrateItem)lane.Item).Definition;
		CurrentItemRemaining = ShapeCrateItem.SIZE;
		lane.ClearLaneRaw_UNSAFE();
		lane.MaxStep_S = InputLane.Definition.Length_S;
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return (index == 0) ? LowerOutputLane : UpperOutputLane;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, LowerOutputLane);
		BeltSimulation.UpdateLane(options, UpperOutputLane);
		BeltSimulation.UpdateLane(options, InputLane);
		if (CurrentItem != null)
		{
			if (LowerOutputLane.Empty && CurrentItemRemaining > 0)
			{
				CurrentItemRemaining--;
				LowerOutputLane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(CurrentItem.Hash);
				LowerOutputLane.Progress_T = 0;
			}
			if (UpperOutputLane.Empty && CurrentItemRemaining > 0)
			{
				CurrentItemRemaining--;
				UpperOutputLane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(CurrentItem.Hash);
				UpperOutputLane.Progress_T = 0;
			}
		}
		if (CurrentItemRemaining <= 0)
		{
			CurrentItem = null;
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, UpperOutputLane);
		DrawDynamic_BeltLane(options, LowerOutputLane);
		float progress = 0f;
		if (CurrentItem != null)
		{
			progress = math.saturate((float)CurrentItemRemaining / (float)ShapeCrateItem.SIZE);
		}
		if (!(progress <= 0.01f))
		{
			DrawDynamic_RawShape(options, CurrentItem, new float3(-0.119f, 0f, 0.905f));
			DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0.248f, -0.2428f, 0.89397f), new float3(1f, progress, 1f));
		}
	}
}
