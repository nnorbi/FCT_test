using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Unity.Mathematics;

public static class BuildingMetadataExporter
{
	private struct V3Int
	{
		public int X;

		public int Y;

		public int Z;

		public V3Int(int3 v)
		{
			X = v.x;
			Y = v.y;
			Z = v.z;
		}
	}

	private struct V3Float
	{
		public float X;

		public float Y;

		public float Z;

		public V3Float(float3 v)
		{
			X = v.x;
			Y = v.y;
			Z = v.z;
		}
	}

	private class MetaBuildingIOExport
	{
		public V3Int Position_L;

		public Grid.Direction Direction_L;

		public static MetaBuildingIOExport Convert(MetaBuildingInternalVariant.BaseIO io)
		{
			return new MetaBuildingIOExport
			{
				Position_L = new V3Int((int3)io.Position_L),
				Direction_L = io.Direction_L
			};
		}
	}

	private class MetaFluidContainerExport
	{
		public string Name;

		public float Max;

		public bool AllowDrain;

		public bool AllowGain;

		public V3Float Position_L;

		public MetaBuildingIOExport[] Connections;

		public static MetaFluidContainerExport Convert(MetaBuildingInternalVariant.FluidContainerConfig container)
		{
			MetaFluidContainerExport metaFluidContainerExport = new MetaFluidContainerExport();
			metaFluidContainerExport.Name = container.Name;
			metaFluidContainerExport.Max = container.Max;
			metaFluidContainerExport.AllowDrain = container.AllowDrain;
			metaFluidContainerExport.AllowGain = container.AllowGain;
			metaFluidContainerExport.Position_L = new V3Float(container.Position_L);
			metaFluidContainerExport.Connections = container.Connections.Select(MetaBuildingIOExport.Convert).ToArray();
			return metaFluidContainerExport;
		}
	}

	private class MetaBuildingInternalVariantExport
	{
		public string Id;

		public string MirroredInternalVariantId;

		public V3Int[] Tiles;

		public MetaBuildingIOExport[] BeltInputs;

		public MetaBuildingIOExport[] BeltOutputs;

		public MetaFluidContainerExport[] FluidContainers;

		public static MetaBuildingInternalVariantExport Convert(MetaBuildingInternalVariant internalVariant)
		{
			MetaBuildingInternalVariantExport metaBuildingInternalVariantExport = new MetaBuildingInternalVariantExport();
			metaBuildingInternalVariantExport.Id = internalVariant.name;
			metaBuildingInternalVariantExport.MirroredInternalVariantId = internalVariant.MirroredInternalVariant?.name;
			metaBuildingInternalVariantExport.Tiles = internalVariant.Tiles.Select((TileDirection t) => new V3Int((int3)t)).ToArray();
			metaBuildingInternalVariantExport.BeltInputs = internalVariant.BeltInputs.Select(MetaBuildingIOExport.Convert).ToArray();
			metaBuildingInternalVariantExport.BeltOutputs = internalVariant.BeltOutputs.Select(MetaBuildingIOExport.Convert).ToArray();
			metaBuildingInternalVariantExport.FluidContainers = internalVariant.FluidContainers.Select(MetaFluidContainerExport.Convert).ToArray();
			return metaBuildingInternalVariantExport;
		}
	}

	private class MetaBuildingVariantExport
	{
		public string Id;

		public string Title;

		public bool Removable;

		public bool Selectable;

		public bool PlayerBuildable;

		public bool AutoConnectBelts;

		public string PipetteOverrideVariantId;

		public MetaBuildingInternalVariantExport[] InternalVariants;

		public static MetaBuildingVariantExport Convert(MetaBuildingVariant variant)
		{
			MetaBuildingVariantExport metaBuildingVariantExport = new MetaBuildingVariantExport();
			metaBuildingVariantExport.Id = variant.name;
			metaBuildingVariantExport.Title = variant.Title;
			metaBuildingVariantExport.Removable = variant.Removable;
			metaBuildingVariantExport.Selectable = variant.Selectable;
			metaBuildingVariantExport.PlayerBuildable = variant.PlayerBuildable;
			metaBuildingVariantExport.AutoConnectBelts = variant.AutoConnectBelts;
			metaBuildingVariantExport.PipetteOverrideVariantId = variant.PipetteOverride?.name;
			metaBuildingVariantExport.InternalVariants = variant.InternalVariants.Select(MetaBuildingInternalVariantExport.Convert).ToArray();
			return metaBuildingVariantExport;
		}
	}

	private struct MetaBuildingExport
	{
		public string Id;

		public string Title;

		public MetaBuildingVariantExport[] Variants;

		public static MetaBuildingExport Convert(MetaBuilding building)
		{
			return new MetaBuildingExport
			{
				Id = building.name,
				Title = building.Title,
				Variants = building.Variants.Where((MetaBuildingVariant v) => v.PlayerBuildable).Select(MetaBuildingVariantExport.Convert).ToArray()
			};
		}
	}

	public static void ExportMetadata()
	{
		MetaBuildingExport[] buildings = Singleton<GameCore>.G.Mode.Buildings.Select(MetaBuildingExport.Convert).ToArray();
		string json = JsonConvert.SerializeObject(buildings, SavegameSerializerBase.JSON_SETTINGS);
		File.WriteAllText(Path.Join(GameEnvironmentManager.DATA_PATH, "buildings-metadata.json"), json);
	}
}
