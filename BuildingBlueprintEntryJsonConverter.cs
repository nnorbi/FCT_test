using System;
using Global.Core.Exceptions;
using Global.Core.Serialization.Json;
using Newtonsoft.Json;
using Unity.Mathematics;

public class BuildingBlueprintEntryJsonConverter : ExceptionHandlingJsonConverter<BuildingBlueprint.Entry>
{
	private struct SerializableBlueprintEntry
	{
		public int X;

		public int Y;

		public int L;

		public int R;

		public string T;

		public byte[] C;
	}

	public BuildingBlueprintEntryJsonConverter(IExceptionHandler exceptionHandler)
		: base(exceptionHandler)
	{
	}

	public override void WriteJson(JsonWriter writer, BuildingBlueprint.Entry entry, Newtonsoft.Json.JsonSerializer serializer)
	{
		try
		{
			SerializableBlueprintEntry serializableEntry = new SerializableBlueprintEntry
			{
				X = entry.Tile_L.x,
				Y = entry.Tile_L.y,
				L = entry.Tile_L.z,
				T = entry.InternalVariant.name,
				R = (int)entry.Rotation,
				C = ((entry.AdditionalConfigData.Length == 0) ? null : entry.AdditionalConfigData)
			};
			serializer.Serialize(writer, serializableEntry);
		}
		catch (Exception exception)
		{
			HandleException(exception);
		}
	}

	public override BuildingBlueprint.Entry ReadJson(JsonReader reader, Type objectType, BuildingBlueprint.Entry existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
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
		if (!mode.TryGetBuildingInternalVariant(entry.T, out var internalVariant))
		{
			HandleException(new BlueprintSerializationUnknownTypeException(entry.T));
			return null;
		}
		return new BuildingBlueprint.Entry
		{
			Tile_L = new TileDirection(entry.X, entry.Y, (short)math.max(entry.L, 0)),
			Rotation = (Grid.Direction)FastMath.SafeMod(entry.R, 4),
			InternalVariant = internalVariant,
			AdditionalConfigData = (entry.C ?? Array.Empty<byte>())
		};
	}
}
