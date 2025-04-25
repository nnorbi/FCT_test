using System;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable]
public class SplitterEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane[] OutputLanes;

	protected BeltLane InputLane;

	protected int NextPreferredLane = 0;

	public SplitterEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition[] definitions = InternalVariant.BeltLaneDefinitions;
		InputLane = new BeltLane(definitions[0]);
		OutputLanes = new BeltLane[InternalVariant.BeltOutputs.Length];
		for (int i = 0; i < OutputLanes.Length; i++)
		{
			OutputLanes[i] = new BeltLane(definitions[1 + i]);
		}
		InputLane.NextLane = OutputLanes[0];
		InputLane.MaxStepClampHook = delegate(BeltLane lane, int maxStep_S)
		{
			int num = maxStep_S;
			for (int j = 0; j < OutputLanes.Length; j++)
			{
				BeltLane beltLane = OutputLanes[j];
				num = ((!beltLane.HasItem) ? math.max(beltLane.MaxStep_S, num) : math.max(num, beltLane.Definition.S_From_T(beltLane.Progress_T)));
			}
			return num;
		};
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		visitor.SyncInt_4(ref NextPreferredLane);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLane;
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLanes[index];
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		List<BeltLane> lanes = new List<BeltLane>();
		lanes.Add(InputLane);
		BeltLane[] outputLanes = OutputLanes;
		foreach (BeltLane lane in outputLanes)
		{
			lanes.Add(lane);
		}
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, InputLane),
			new HUDSidePanelModuleBeltItemContents(lanes)
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		BeltLane nextLane = null;
		for (int i = 0; i < OutputLanes.Length; i++)
		{
			int index = (i + NextPreferredLane) % OutputLanes.Length;
			BeltLane lane = OutputLanes[index];
			BeltSimulation.UpdateLane(options, lane);
			if (nextLane == null && lane.Empty)
			{
				nextLane = lane;
			}
		}
		InputLane.NextLane = nextLane;
		if (BeltSimulation.UpdateLane(options, InputLane))
		{
			NextPreferredLane = (NextPreferredLane + 1) % OutputLanes.Length;
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		for (int i = 0; i < OutputLanes.Length; i++)
		{
			DrawDynamic_BeltLane(options, OutputLanes[i]);
		}
		DrawDynamic_BeltLane(options, InputLane);
	}
}
