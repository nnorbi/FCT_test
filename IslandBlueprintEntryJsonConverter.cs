using System;
using Global.Core.Exceptions;
using Global.Core.Serialization.Json;
using Newtonsoft.Json;

public class IslandBlueprintEntryJsonConverter : ExceptionHandlingJsonConverter<IslandBlueprint.Entry>
{
	private struct SerializableBlueprintEntry
	{
		public int X;

		public int Y;

		public int R;

		public string T;

		public BuildingBlueprint B;
	}

	public IslandBlueprintEntryJsonConverter(IExceptionHandler exceptionHandler)
		: base(exceptionHandler)
	{
	}

	public override void WriteJson(JsonWriter writer, IslandBlueprint.Entry entry, Newtonsoft.Json.JsonSerializer serializer)
	{
		try
		{
			SerializableBlueprintEntry serializableEntry = new SerializableBlueprintEntry
			{
				X = entry.Chunk_L.x,
				Y = entry.Chunk_L.y,
				T = entry.Layout.name,
				R = (int)entry.Rotation,
				B = entry.BuildingBlueprint
			};
			serializer.Serialize(writer, serializableEntry);
		}
		catch (Exception exception)
		{
			HandleException(exception);
		}
	}

	public override IslandBlueprint.Entry ReadJson(JsonReader reader, Type objectType, IslandBlueprint.Entry existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
	{
		SerializableBlueprintEntry entry;
		try
		{
			entry = serializer.Deserialize<SerializableBlueprintEntry>(reader);
		}
		catch (Exception innerException)
		{
			HandleException(new BlueprintSerializationJsonException("Failed to deserialize SerializableBlueprintEntry json.", innerException));
			return null;
		}
		GameModeHandle mode = Singleton<GameCore>.G.Mode;
		if (!mode.TryGetMetaIslandLayout(entry.T, out var layout))
		{
			HandleException(new BlueprintSerializationUnknownTypeException(entry.T));
			return null;
		}
		return new IslandBlueprint.Entry
		{
			Chunk_L = new ChunkDirection(entry.X, entry.Y),
			Rotation = (Grid.Direction)FastMath.SafeMod(entry.R, 4),
			Layout = layout,
			BuildingBlueprint = entry.B
		};
	}
}
