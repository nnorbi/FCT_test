using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Global.Core.Exceptions;
using Global.Core.Serialization.Json;
using ICSharpCode.SharpZipLib.GZip;
using Newtonsoft.Json;

public static class BlueprintSerializer
{
	private struct Serialized
	{
		public int V;

		public IBlueprint BP;
	}

	public const string PREFIX = "SHAPEZ2-";

	public const string CONTENT_DIVIDER = "-";

	public const string SUFFIX = "$";

	private static CollectingExceptionHandler JSON_SERIALIZATION_EXCEPTION_HANDLER = new CollectingExceptionHandler();

	private static Global.Core.Serialization.Json.JsonSerializer JSON_SERIALIZER;

	public static int VERSION = 1;

	public static int MIN_VERSION = 1;

	public static void InitJsonConfig()
	{
		JsonSerializerConfiguration jsonSerializerConfiguration = new JsonSerializerConfiguration();
		jsonSerializerConfiguration.Converters = new List<JsonConverter>
		{
			new BuildingBlueprintEntryJsonConverter(JSON_SERIALIZATION_EXCEPTION_HANDLER),
			new IslandBlueprintEntryJsonConverter(JSON_SERIALIZATION_EXCEPTION_HANDLER),
			new BlueprintJsonConverter()
		};
		jsonSerializerConfiguration.SerializationBinder = new ExplicitTypeBinder(("Building", typeof(SerializableBuildingBlueprint)), ("Island", typeof(SerializableIslandBlueprint)));
		JsonSerializerConfiguration configuration = jsonSerializerConfiguration;
		JSON_SERIALIZER = new Global.Core.Serialization.Json.JsonSerializer(configuration);
	}

	public static string Serialize(IBlueprint blueprint)
	{
		Serialized serialized = new Serialized
		{
			V = Savegame.VERSION,
			BP = blueprint
		};
		JSON_SERIALIZATION_EXCEPTION_HANDLER.Clear();
		string json = JSON_SERIALIZER.Serialize(serialized);
		using MemoryStream memIn = new MemoryStream(SavegameSerializerBase.ENCODING.GetBytes(json));
		using MemoryStream memOut = new MemoryStream();
		GZip.Compress(memIn, memOut, isStreamOwner: true, 1024, 9);
		byte[] rawString = memOut.ToArray();
		string base64String = Convert.ToBase64String(rawString);
		return "SHAPEZ2-" + VERSION + "-" + base64String + "$";
	}

	private static Serialized DeserializeCompressed(string contentBase64, out Exception[] occuredJsonExceptions)
	{
		byte[] content;
		try
		{
			content = Convert.FromBase64String(contentBase64);
		}
		catch (Exception innerException)
		{
			throw new BlueprintSerializationConvertBase64Exception("Failed to convert Serialized from 64 base.", innerException);
		}
		using MemoryStream memIn = new MemoryStream(content);
		using MemoryStream memOut = new MemoryStream();
		try
		{
			GZip.Decompress(memIn, memOut, isStreamOwner: true);
		}
		catch (Exception innerException2)
		{
			throw new BlueprintSerializationZipException("Failed to decompress Serialized gzip.", innerException2);
		}
		byte[] decompressed = memOut.GetBuffer();
		string json = SavegameSerializerBase.ENCODING.GetString(decompressed);
		if (!json.Contains("\"$type\""))
		{
			json = json.Replace("\"BP\":{", "\"BP\":{\"$type\":\"Building\",");
		}
		try
		{
			JSON_SERIALIZATION_EXCEPTION_HANDLER.Clear();
			Serialized deserializedObject = JSON_SERIALIZER.Deserialize<Serialized>(json);
			occuredJsonExceptions = JSON_SERIALIZATION_EXCEPTION_HANDLER.CollectedExceptions.ToArray();
			return deserializedObject;
		}
		catch (Exception innerException3)
		{
			throw new BlueprintSerializationJsonException("Failed to deserialize Serialized json.", innerException3);
		}
	}

	public static IBlueprint Deserialize(string serialized)
	{
		if (!TryDeserialize(serialized, out var blueprint, out var exception))
		{
			throw exception;
		}
		return blueprint;
	}

	public static bool TryDeserialize(string serializedBlueprint, out IBlueprint blueprint, out BlueprintException exception, bool trySanitize = false)
	{
		blueprint = null;
		List<BlueprintException> sanitizationExceptions = new List<BlueprintException>();
		try
		{
			if (!serializedBlueprint.StartsWith("SHAPEZ2-"))
			{
				exception = new BlueprintSerializationSyntaxException("SHAPEZ2-");
				return false;
			}
			if (!serializedBlueprint.EndsWith("$"))
			{
				exception = new BlueprintSerializationSyntaxException("$");
				return false;
			}
			int contentStart = serializedBlueprint.IndexOf("-", "SHAPEZ2-".Length, StringComparison.Ordinal);
			if (contentStart < 0)
			{
				exception = new BlueprintSerializationSyntaxException("-");
				return false;
			}
			string versionText = serializedBlueprint.Substring("SHAPEZ2-".Length, contentStart - "SHAPEZ2-".Length);
			if (!int.TryParse(versionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var versionNumber))
			{
				exception = new BlueprintSerializationParsingException("blueprint-version", versionText);
				return false;
			}
			if (versionNumber < MIN_VERSION || versionNumber > VERSION)
			{
				exception = new BlueprintSerializationBlueprintVersionException(versionNumber, MIN_VERSION, VERSION);
				if (!trySanitize)
				{
					return false;
				}
				sanitizationExceptions.Add(exception);
			}
			string contentBase64 = serializedBlueprint.Substring(contentStart + "-".Length, serializedBlueprint.Length - contentStart - "$".Length - "-".Length);
			Exception[] occuredJsonExceptions;
			Serialized data = DeserializeCompressed(contentBase64, out occuredJsonExceptions);
			bool sanitizeConfigData = false;
			if (data.V < Savegame.LOWEST_SUPPORTED_VERSION || data.V > Savegame.VERSION)
			{
				sanitizeConfigData = true;
				exception = new BlueprintSerializationSavegameVersionException(data.V, Savegame.LOWEST_SUPPORTED_VERSION, Savegame.VERSION);
				if (!trySanitize)
				{
					return false;
				}
				sanitizationExceptions.Add(exception);
			}
			if (trySanitize)
			{
				sanitizationExceptions.AddRange(occuredJsonExceptions.Cast<BlueprintException>());
			}
			blueprint = data.BP;
		}
		catch (BlueprintException ex)
		{
			exception = ex;
			return false;
		}
		catch (Exception innerException)
		{
			exception = new UnexpectedBlueprintException(innerException);
			return false;
		}
		exception = ((sanitizationExceptions.Count > 0) ? new AggregateBlueprintException(sanitizationExceptions) : null);
		return true;
	}
}
