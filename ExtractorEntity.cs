#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class ExtractorEntity : MapEntity<MetaBuildingInternalVariant>
{
	protected BeltLane ProcessingLane;

	protected BeltLane OutputLane;

	protected BeltItem _ResourceItem = null;

	public BeltItem ResourceItem => _ResourceItem;

	public ExtractorEntity(CtorArgs payload)
		: base(payload)
	{
		OutputLane = new BeltLane(InternalVariant.BeltLaneDefinitions[1]);
		ProcessingLane = new BeltLane(InternalVariant.BeltLaneDefinitions[0], OutputLane);
	}

	protected override void Belts_TraverseAdditionalLanes(IBeltLaneTraverser traverser)
	{
		traverser.Traverse(ProcessingLane);
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		return OutputLane;
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, OutputLane),
			new HUDSidePanelModuleBeltItemContents(new List<BeltLane> { OutputLane })
		};
	}

	public new static float HUD_GetProcessingDurationRaw(MetaBuildingInternalVariant internalVariant)
	{
		return internalVariant.BeltLaneDefinitions[0].Duration / 2f;
	}

	public new static HUDSidePanelModuleBaseStat[] HUD_ComputeStats(MetaBuildingInternalVariant internalVariant)
	{
		return new HUDSidePanelModuleBaseStat[1] { MapEntity.HUD_CreateProcessingTimeStat(HUD_GetProcessingDurationRaw(internalVariant), MapEntity.HUD_GetResearchSpeed(internalVariant)) };
	}

	protected override void Hook_OnCreated()
	{
		_ResourceItem = Island.GetTileInfo_UNSAFE_I(in Tile_I).BeltResource;
		ProcessingLane.Item = _ResourceItem;
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		if (_ResourceItem != null)
		{
			BeltSimulation.UpdateLane(options, OutputLane);
			if (BeltSimulation.UpdateLane(options, ProcessingLane))
			{
				ProcessingLane.Item = _ResourceItem;
				ProcessingLane.Progress_T = ProcessingLane.MaxTickClamped_T;
			}
			if (ProcessingLane.Empty)
			{
				IslandTileCoordinate tile_I = Tile_I;
				Debug.LogError("Extractor item @ " + tile_I.ToString() + " vanished on both lanes!");
				CreateItemOnProcessingLane();
			}
		}
	}

	public override void Belts_ClearContents()
	{
		base.Belts_ClearContents();
		Debug.Assert(ProcessingLane.Empty);
		CreateItemOnProcessingLane();
	}

	private void CreateItemOnProcessingLane()
	{
		Debug.Assert(!ProcessingLane.HasItem);
		ProcessingLane.Item = _ResourceItem;
		ProcessingLane.Progress_T = 0;
		ProcessingLane.MaxStep_S = 0;
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		if (_ResourceItem == null)
		{
			IslandTileCoordinate tile_I = Tile_I;
			Debug.LogWarning("Extractor resource item is null on extractor @" + tile_I.ToString() + " island " + Island.Descriptor.ToString());
			return;
		}
		float pusherReturnProgress = (ProcessingLane.Progress - 0.8f) / 0.19999999f;
		if (ProcessingLane.HasItem)
		{
			float3 processingPos_L = math.lerp(ProcessingLane.Definition.ItemStartPos_L, ProcessingLane.Definition.ItemEndPos_L, InternalVariant.GetCurve(0, (ProcessingLane.Progress - 0.5f) * 2f));
			DrawDynamic_BeltItem(options, _ResourceItem, in processingPos_L);
		}
		float3 pos_L = math.lerp(OutputLane.Definition.ItemStartPos_L, OutputLane.Definition.ItemEndPos_L, OutputLane.Progress);
		if (OutputLane.HasItem)
		{
			DrawDynamic_BeltItem(options, OutputLane.Item, in pos_L);
		}
		float pusherProgress;
		if (OutputLane.Progress < 0.8f)
		{
			pusherProgress = pos_L.x;
		}
		else
		{
			float pusherBasePosition = pos_L.x;
			pusherProgress = InternalVariant.GetCurve(1, pusherReturnProgress) * pusherBasePosition;
		}
		DrawDynamic_Mesh(options, InternalVariant.SupportMeshesInternalLOD[0], new float3(-0.325f + pusherProgress, 0f, 0f));
	}
}
