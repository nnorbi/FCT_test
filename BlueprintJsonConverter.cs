using System;
using System.Linq;
using Newtonsoft.Json;

public class BlueprintJsonConverter : JsonConverter<IBlueprint>
{
	public override void WriteJson(JsonWriter writer, IBlueprint blueprint, JsonSerializer serializer)
	{
		ISerializableBlueprint serializableBlueprint;
		if (blueprint is BuildingBlueprint buildingBlueprint)
		{
			serializableBlueprint = new SerializableBuildingBlueprint
			{
				Entries = buildingBlueprint.Entries
			};
		}
		else
		{
			if (!(blueprint is IslandBlueprint islandBlueprint))
			{
				throw new NotImplementedException(string.Format("Unsupported {0} type '{1}.'", "IBlueprint", blueprint.GetType()));
			}
			serializableBlueprint = new SerializableIslandBlueprint
			{
				Entries = islandBlueprint.Entries
			};
		}
		serializer.Serialize(writer, serializableBlueprint, typeof(ISerializableBlueprint));
	}

	public override IBlueprint ReadJson(JsonReader reader, Type objectType, IBlueprint existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		ISerializableBlueprint serializableBlueprint = serializer.Deserialize<ISerializableBlueprint>(reader);
		if (serializableBlueprint is SerializableBuildingBlueprint serializableBuildingBlueprint)
		{
			return BuildingBlueprint.FromEntriesModifyInPlace(serializableBuildingBlueprint.Entries.Where((BuildingBlueprint.Entry e) => e != null), recomputeOrigin: false);
		}
		if (serializableBlueprint is SerializableIslandBlueprint serializableIslandBlueprint)
		{
			return IslandBlueprint.FromEntriesModifyInPlace(serializableIslandBlueprint.Entries.Where((IslandBlueprint.Entry e) => e != null), recomputeOrigin: true);
		}
		throw new NotImplementedException(string.Format("Unsupported {0} type '{1}.'", "ISerializableBlueprint", serializableBlueprint.GetType()));
	}
}
