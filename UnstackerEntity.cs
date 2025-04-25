using System;
using System.Collections.Generic;

[Serializable]
public class UnstackerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane InputLane;

	protected BeltLane FakeProcessingLane;

	protected BeltLane LowerOutputLane;

	protected BeltLane UpperOutputLane;

	public UnstackerEntity(CtorArgs payload)
		: base(payload)
	{
		LowerOutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[2]);
		UpperOutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[3]);
		FakeProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1], LowerOutputLane);
		InputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], FakeProcessingLane);
		FakeProcessingLane.PreAcceptHook = (BeltItem item) => (LowerOutputLane.HasItem || UpperOutputLane.HasItem) ? null : item;
		LowerOutputLane.PostAcceptHook = delegate(BeltLane lowerLane, ref int remainingTicks_T)
		{
			ShapeItem shapeItem = (ShapeItem)lowerLane.Item;
			ShapeUnstackResult shapeUnstackResult = Singleton<GameCore>.G.Shapes.Op_Unstack.Execute(shapeItem.Definition);
			lowerLane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeUnstackResult.LowerPart);
			if (lowerLane.Item == null)
			{
				lowerLane.Progress_T = 0;
				lowerLane.MaxStep_S = lowerLane.ComputeMaxStepWhenEmptyINTERNAL_S();
			}
			UpperOutputLane.Item = Singleton<GameCore>.G.Shapes.GetItemByHash(shapeUnstackResult.UpperPart);
			UpperOutputLane.Progress_T = 0;
		};
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(FakeProcessingLane);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return (index == 0) ? LowerOutputLane : UpperOutputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { InputLane, LowerOutputLane, UpperOutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration + internalVariant.BeltLaneDefinitions[1].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltSimulation.UpdateLane(options, LowerOutputLane);
		BeltSimulation.UpdateLane(options, UpperOutputLane);
		BeltSimulation.UpdateLane(options, FakeProcessingLane);
		BeltSimulation.UpdateLane(options, InputLane);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		DrawDynamic_BeltLane(options, InputLane);
		DrawDynamic_BeltLane(options, LowerOutputLane);
		DrawDynamic_BeltLane(options, UpperOutputLane);
	}
}
