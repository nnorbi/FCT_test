using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.Collections.Scoped;
using UnityEngine;

[Serializable]
public class IslandBuildings
{
	protected class CheckForDuplicatesBeltLaneTraverser : IBeltLaneTraverser
	{
		protected HashSet<BeltLane> SeenLanes = new HashSet<BeltLane>();

		public void Traverse(BeltLane lane)
		{
			if (SeenLanes.Contains(lane))
			{
				throw new Exception("Duplicate belt lane returned: " + lane.Definition.Name);
			}
			SeenLanes.Add(lane);
		}
	}

	[SerializeReference]
	public List<MapEntity> Buildings = new List<MapEntity>();

	public List<MapEntity> BuildingDrawQueueIsland = new List<MapEntity>();

	public List<MapEntity> BuildingDrawQueueAlways = new List<MapEntity>();

	[NonSerialized]
	public Island Island;

	protected MapEntity[] UpdateOrder = null;

	protected FluidNetwork[] FluidNetworks;

	public IEnumerable<MapEntity> BuildingsInUpdateOrder
	{
		get
		{
			IEnumerable<MapEntity> updateOrder = UpdateOrder;
			return updateOrder ?? Buildings;
		}
	}

	public IslandBuildings(Island island)
	{
		Island = island;
	}

	public void Serialize(ISerializationVisitor visitor)
	{
		visitor.WriteInt_4(Buildings.Count);
		for (int i = 0; i < Buildings.Count; i++)
		{
			MapEntity building = Buildings[i];
			string internalVariantName = building.InternalVariant.name;
			visitor.Checkpoint("island-buildings.building.pre");
			visitor.WriteShort_2(building.Tile_I.x);
			visitor.WriteShort_2(building.Tile_I.y);
			visitor.WriteByte_1((byte)building.Tile_I.z);
			visitor.WriteByte_1((byte)building.Rotation_G);
			visitor.WriteString_4(internalVariantName);
			visitor.Checkpoint("island-buildings.building.beforehook-" + internalVariantName);
			building.Serialization_SyncContents(visitor);
			visitor.Checkpoint("island-buildings.building.afterhook-" + internalVariantName);
			building.Serialization_SyncConfig(visitor);
			visitor.Checkpoint("island-buildings.building.afterhook-config-" + internalVariantName);
		}
		LateHooks_Sync(visitor);
	}

	public void Deserialize(ISerializationVisitor visitor)
	{
		int count = visitor.ReadInt_4();
		GameModeHandle mode = Singleton<GameCore>.G.Mode;
		IslandTileCoordinate tile_I = default(IslandTileCoordinate);
		for (int i = 0; i < count; i++)
		{
			visitor.Checkpoint("island-buildings.building.pre");
			tile_I.x = visitor.ReadShort_2();
			tile_I.y = visitor.ReadShort_2();
			tile_I.z = visitor.ReadByte_1();
			Grid.Direction rotation_G = (Grid.Direction)visitor.ReadByte_1();
			string internalVariantName = visitor.ReadString_4();
			MetaBuildingInternalVariant internalVariant = mode.GetBuildingInternalVariant(internalVariantName);
			MapEntity entity = Island.Map.CreateEntity(internalVariant, Island.Descriptor, tile_I, rotation_G, skipChecks: true);
			visitor.Checkpoint("island-buildings.building.beforehook-" + internalVariantName);
			entity.Serialization_SyncContents(visitor);
			visitor.Checkpoint("island-buildings.building.afterhook-" + internalVariantName);
			entity.Serialization_SyncConfig(visitor);
			visitor.Checkpoint("island-buildings.building.afterhook-config-" + internalVariantName);
		}
		LateHooks_Sync(visitor);
		ValidateBeltLanes();
	}

	protected void LateHooks_Sync(ISerializationVisitor visitor)
	{
		visitor.Checkpoint("island-buildings.late-hooks.begin");
		for (int i = 0; i < Buildings.Count; i++)
		{
			MapEntity building = Buildings[i];
			string internalVariantName = building.InternalVariant.name;
			visitor.Checkpoint("island-buildings.building.beforelatehook-" + internalVariantName);
			building.Serialization_SyncLate(visitor);
			visitor.Checkpoint("island-buildings.building.afterlatehook-" + internalVariantName);
		}
		visitor.Checkpoint("island-buildings.late-hooks.end");
	}

	public byte[] LateHooks_Serialize()
	{
		using MemoryStream stream = new MemoryStream();
		BinarySerializationVisitor serializer = new BinarySerializationVisitor(writing: true, checkpoints: false, Savegame.VERSION, stream);
		LateHooks_Sync(serializer);
		stream.Flush();
		return stream.ToArray();
	}

	public void LateHooks_Deserialize(byte[] data)
	{
		using MemoryStream stream = new MemoryStream(data);
		BinarySerializationVisitor serializer = new BinarySerializationVisitor(writing: false, checkpoints: false, Savegame.VERSION, stream);
		LateHooks_Sync(serializer);
	}

	public void RegisterEntityInternal(MapEntity entity, bool skipChecks = false)
	{
		if (!skipChecks)
		{
			ValidateBeltLanes();
		}
		Island.LinkEntity(entity);
		Buildings.Add(entity);
		entity.OnCreated();
		if (!skipChecks)
		{
			ValidateBeltLanes();
		}
		switch (entity.Order_GetCullMode())
		{
		case MapEntity.Drawing_CullMode.DrawAlways_NEEDS_MANUAL_CULLING:
			BuildingDrawQueueAlways.Add(entity);
			break;
		case MapEntity.Drawing_CullMode.DrawWhenIslandInView:
			BuildingDrawQueueIsland.Add(entity);
			break;
		}
		UpdateOrder = null;
		FluidNetworks = null;
	}

	public void UnregisterEntityInternal(MapEntity entity)
	{
		ValidateBeltLanes();
		Island.UnlinkEntity(entity);
		Buildings.Remove(entity);
		entity.OnDestroyed();
		ValidateBeltLanes();
		switch (entity.Order_GetCullMode())
		{
		case MapEntity.Drawing_CullMode.DrawAlways_NEEDS_MANUAL_CULLING:
			BuildingDrawQueueAlways.Remove(entity);
			break;
		case MapEntity.Drawing_CullMode.DrawWhenIslandInView:
			BuildingDrawQueueIsland.Remove(entity);
			break;
		}
		UpdateOrder = null;
		FluidNetworks = null;
	}

	public void RemoveAllOnIslandRemove()
	{
		foreach (MapEntity entity in Buildings)
		{
			Island.UnlinkEntity(entity);
			entity.OnDestroyed();
		}
		Cleanup();
	}

	protected void DoComputeUpdateOrder(HashSet<MapEntity> alreadyUpdated, List<MapEntity> normalBuildings, MapEntity entity)
	{
		if (alreadyUpdated.Contains(entity))
		{
			return;
		}
		alreadyUpdated.Add(entity);
		HashSet<MapEntity> dependencies = entity.Belts_GetDependencies();
		foreach (MapEntity dependencyEntity in dependencies)
		{
			if (dependencyEntity.Island == Island)
			{
				DoComputeUpdateOrder(alreadyUpdated, normalBuildings, dependencyEntity);
			}
		}
		switch (entity.Order_ComputeUpdateMode())
		{
		case MapEntity.Config_UpdateMode.Normal:
			normalBuildings.Add(entity);
			break;
		}
	}

	protected MapEntity[] ComputeUpdateOrder()
	{
		using ScopedList<MapEntity> normalBuildings = ScopedList<MapEntity>.Get();
		using ScopedHashSet<MapEntity> updated = ScopedHashSet<MapEntity>.Get();
		foreach (MapEntity building in Buildings)
		{
			DoComputeUpdateOrder(updated, normalBuildings, building);
		}
		return normalBuildings.ToArray();
	}

	protected FluidNetwork[] ComputeFluidNetworks()
	{
		using ScopedHashSet<FluidNetwork> networks = ScopedHashSet<FluidNetwork>.Get();
		for (int i = 0; i < Buildings.Count; i++)
		{
			MapEntity building = Buildings[i];
			MetaBuildingInternalVariant.FluidContainerConfig[] containers = building.InternalVariant.FluidContainers;
			for (int j = 0; j < containers.Length; j++)
			{
				FluidContainer container = building.Fluids_GetContainerByIndex(j);
				if (container.Network == null)
				{
					string name = building.InternalVariant.name;
					IslandTileCoordinate tile_I = building.Tile_I;
					throw new Exception("Can not add network from container: network==null on " + name + " @ " + tile_I.ToString());
				}
				networks.Add(container.Network);
			}
		}
		return networks.ToArray();
	}

	public void Update(TickOptions options)
	{
		if (UpdateOrder == null)
		{
			UpdateOrder = ComputeUpdateOrder();
		}
		if (FluidNetworks == null)
		{
			FluidNetworks = ComputeFluidNetworks();
		}
		int networkCount = FluidNetworks.Length;
		for (int i = 0; i < networkCount; i++)
		{
			FluidNetworks[i].Update(options);
		}
		int entityCount = UpdateOrder.Length;
		for (int j = 0; j < entityCount; j++)
		{
			UpdateOrder[j].OnUpdate(options);
		}
	}

	public void Cleanup()
	{
		Buildings.Clear();
		BuildingDrawQueueAlways.Clear();
		BuildingDrawQueueIsland.Clear();
		FluidNetworks = null;
		UpdateOrder = null;
	}

	protected void ValidateBeltLanes()
	{
	}
}
