using System;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public abstract class BeltEntity : MapEntity<MetaBuildingInternalVariant>
{
	[SerializeReference]
	public BeltPath Path = null;

	public int IndexInPath = -1;

	public BeltEntity(CtorArgs payload)
		: base(payload)
	{
	}

	protected override void Hook_OnCreated()
	{
		BeltPath sourcePath = null;
		BeltPath destinationPath = null;
		Belts_LinkedEntity[] inputs = Belts_GetInputConnections();
		Belts_LinkedEntity[] outputs = Belts_GetOutputConnections();
		if (inputs[0].Entity != null && inputs[0].Entity is BeltEntity)
		{
			sourcePath = ((BeltEntity)inputs[0].Entity).Path;
		}
		if (outputs[0].Entity != null && outputs[0].Entity is BeltEntity)
		{
			destinationPath = ((BeltEntity)outputs[0].Entity).Path;
		}
		BeltPath.Modify_IntegrateBeltIntoPath(this, sourcePath, destinationPath);
		if (Path == null)
		{
			throw new Exception("Belt path = null");
		}
		if (IndexInPath == -1)
		{
			throw new Exception("Path not assigned after init");
		}
	}

	public override BeltItem Belts_GetPredictedInput(int index)
	{
		return Path.FirstItem;
	}

	public override BeltItem Belts_GetPredictedOutput(int index)
	{
		return Path.LastItem;
	}

	public override void Belts_TraverseLanes(IBeltLaneTraverser traverser)
	{
		if (IndexInPath == -1)
		{
			throw new Exception("Path not yet assigned");
		}
		if (IndexInPath == 0)
		{
			traverser.Traverse(Path.FakeInputLane);
		}
		else if (IndexInPath == Path.Belts.Count - 1)
		{
			traverser.Traverse(Path.FakeOutputLane);
		}
	}

	public override void Serialization_SyncContents(ISerializationVisitor visitor)
	{
	}

	protected override void Hook_SyncLate(ISerializationVisitor visitor)
	{
		if (IndexInPath == -1)
		{
			throw new Exception("Path not yet assigned");
		}
		if (visitor.Writing)
		{
			if (IndexInPath == 0)
			{
				visitor.WriteByte_1((byte)Path.Belts.Count);
				Path.Serialize(visitor);
			}
			else
			{
				visitor.WriteByte_1(0);
			}
			return;
		}
		byte storedPathInformation = visitor.ReadByte_1();
		if (storedPathInformation > 0)
		{
			if ((visitor.Version < 1031) ? ((byte)IndexInPath != 0) : (storedPathInformation != Path.Belts.Count))
			{
				BeltPathLogic dummyLogic = new BeltPathLogic(1000000);
				dummyLogic.Deserialize(visitor);
				IslandTileCoordinate tile_I = Tile_I;
				Debug.LogWarning("Skipping broken path at belt " + tile_I.ToString());
			}
			else
			{
				Path.Deserialize(visitor);
			}
		}
	}

	public abstract float2 GetItemPosition_L(float progress);

	public abstract float GetItemLocalRotation_L(float progress);

	public override BeltLane Belts_GetLaneForInput(int index)
	{
		if (IndexInPath == -1)
		{
			throw new Exception("Path not yet assigned");
		}
		if (IndexInPath == 0)
		{
			return Path.FakeInputLane;
		}
		throw new Exception("GetBeltLaneForInputSlot: is not first belt");
	}

	public override BeltLane Belts_GetLaneForOutput(int index)
	{
		if (IndexInPath == -1)
		{
			throw new Exception("Path not yet assigned");
		}
		if (Path == null)
		{
			throw new Exception("Belt not initialized yet, can't call GetLaneForOutput yet");
		}
		if (IndexInPath == Path.Belts.Count - 1)
		{
			return Path.FakeOutputLane;
		}
		throw new Exception("GetBeltLaneForInputSlot: is not last belt");
	}

	protected override void Belts_LinkLanesAfterCreate()
	{
	}

	protected override void Belts_UnlinkLanesAfterDestroy()
	{
	}

	public override HUDSidePanelModule[] HUD_GetInfoModules()
	{
		return new HUDSidePanelModule[2]
		{
			new HUDSidePanelModuleBuildingEfficiency(this, Path.FakeInputLane),
			new HUDSidePanelModuleBeltItemContents(() => Path.GetItemsAtBeltIndex(IndexInPath))
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

	public override void Belts_ClearContents()
	{
		Path.ClearItemsAtBelt(IndexInPath);
	}

	public override Config_UpdateMode Order_ComputeUpdateMode()
	{
		if (IndexInPath == 0)
		{
			return Config_UpdateMode.Normal;
		}
		return Config_UpdateMode.Never;
	}

	protected override void Hook_OnDestroyed()
	{
		BeltPath.Modify_RemoveBeltFromPath(this);
	}

	protected override void Hook_OnUpdate(TickOptions options)
	{
		if (IndexInPath != 0)
		{
			throw new Exception("Belt index != 0 in update");
		}
		Path.Update(options);
	}

	protected override void Hook_OnDrawDynamic(FrameDrawOptions options)
	{
		Path.Draw(options);
	}
}
