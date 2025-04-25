using System;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable]
public class MergerEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane[] InputLanes;

	protected BeltLane OutputLane;

	protected int CurrentInputIndex = -1;

	protected int NextPreferredInput = 0;

	public MergerEntity(CtorArgs payload)
		: base(payload)
	{
		BeltLaneDefinition[] definitions = InternalVariant.BeltLaneDefinitions;
		OutputLane = new BeltLane(definitions[^1]);
		InputLanes = new BeltLane[InternalVariant.BeltInputs.Length];
		for (int i = 0; i < InputLanes.Length; i++)
		{
			int localIndex = i;
			InputLanes[i] = new BeltLane(definitions[i], OutputLane);
			InputLanes[i].PreAcceptHook = delegate(BeltItem item)
			{
				if (CurrentInputIndex < 0)
				{
					CurrentInputIndex = localIndex;
				}
				return item;
			};
			InputLanes[i].MaxStepClampHook = (BeltLane lane, int maxStep_S) => (CurrentInputIndex == localIndex) ? maxStep_S : 0;
		}
		OutputLane.PostAcceptHook = delegate
		{
			NextPreferredInput = (NextPreferredInput + 1) % InputLanes.Length;
			int currentInputIndex = CurrentInputIndex;
			CurrentInputIndex = -1;
			for (int j = 0; j < InputLanes.Length; j++)
			{
				int num = (NextPreferredInput + j) % InputLanes.Length;
				BeltLane beltLane = InputLanes[num];
				if (!beltLane.Empty && num != currentInputIndex)
				{
					CurrentInputIndex = num;
					break;
				}
			}
		};
	}

	public override void Belts_ClearContents()
	{
		base.Belts_ClearContents();
		CurrentInputIndex = -1;
	}

	protected override void Hook_SyncAdditionalContents(ISerializationVisitor visitor)
	{
		visitor.SyncInt_4(ref CurrentInputIndex);
		visitor.SyncInt_4(ref NextPreferredInput);
	}

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		return InputLanes[index];
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		List<BeltLane> lanes = new List<BeltLane>();
		BeltLane[] inputLanes = InputLanes;
		foreach (BeltLane lane in inputLanes)
		{
			lanes.Add(lane);
		}
		lanes.Add(OutputLane);
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, OutputLane),
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
		BeltSimulation.UpdateLane(options, OutputLane);
		int offset = math.max(0, CurrentInputIndex);
		for (int i = 0; i < InputLanes.Length; i++)
		{
			int index = (i + offset) % InputLanes.Length;
			BeltSimulation.UpdateLane(options, InputLanes[index]);
		}
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		for (int i = 0; i < InputLanes.Length; i++)
		{
			BeltLane lane = InputLanes[i];
			float stopperHeight = 1f;
			if (lane.HasItem)
			{
				stopperHeight = InternalVariant.GetCurve(0, lane.Progress);
				DrawDynamic_BeltLane(options, lane);
			}
			DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(0f, 0f, stopperHeight * 0.06f + 0.001f), Grid.RotateDirection(InternalVariant.BeltInputs[i].Direction_L, Grid.Direction.Left));
		}
		DrawDynamic_BeltLane(options, OutputLane);
	}
}
