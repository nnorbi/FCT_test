using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Global.Core.Serialization.Json;

public class JsonSerializer : ISerializer
{
	private readonly Dictionary<Type, JsonConverter> convertByType = new Dictionary<Type, JsonConverter>();

	protected JsonSerializerSettings SerializerSettings { get; }

	public JsonSerializer()
	{
		SerializerSettings = CreateDefaultSettings(new JsonSerializerConfiguration());
		LoadConverterByType(SerializerSettings.Converters);
	}

	public JsonSerializer(JsonSerializerConfiguration configuration)
	{
		SerializerSettings = CreateDefaultSettings(configuration);
		LoadConverterByType(SerializerSettings.Converters);
	}

	public JsonSerializer(IEnumerable<JsonConverter> converter)
	{
		JsonSerializerConfiguration configuration = new JsonSerializerConfiguration
		{
			Converters = new List<JsonConverter>(converter)
		};
		SerializerSettings = CreateDefaultSettings(configuration);
		LoadConverterByType(SerializerSettings.Converters);
	}

	public JsonSerializer(IEnumerable<Assembly> converterAssemblies)
		: this(converterAssemblies, Enumerable.Empty<JsonConverter>())
	{
	}

	public JsonSerializer(IEnumerable<Assembly> converterAssemblies, IEnumerable<JsonConverter> additionalConverter)
	{
		JsonSerializerConfiguration configuration = new JsonSerializerConfiguration
		{
			Converters = new List<JsonConverter>()
		};
		HashSet<Type> converterTypes = new HashSet<Type>();
		foreach (JsonConverter currentConverter in additionalConverter)
		{
			configuration.Converters.Add(currentConverter);
			converterTypes.Add(currentConverter.GetType());
		}
		foreach (Assembly assembly in converterAssemblies)
		{
			Type[] types = assembly.GetTypes();
			foreach (Type type in types)
			{
				if (!type.IsAbstract && !type.IsInterface && type.IsSubclassOf(typeof(JsonConverter)) && !converterTypes.Contains(type))
				{
					try
					{
						JsonConverter converterInstance = (JsonConverter)Activator.CreateInstance(type);
						configuration.Converters.Add(converterInstance);
						converterTypes.Add(type);
					}
					catch
					{
					}
				}
			}
		}
		SerializerSettings = CreateDefaultSettings(configuration);
		LoadConverterByType(SerializerSettings.Converters);
	}

	public JsonSerializer(JsonSerializerSettings settings)
	{
		SerializerSettings = settings;
		LoadConverterByType(SerializerSettings.Converters);
	}

	public virtual string Serialize(object value)
	{
		return JsonConvert.SerializeObject(value, SerializerSettings);
	}

	public virtual T Deserialize<T>(string value)
	{
		return JsonConvert.DeserializeObject<T>(value, SerializerSettings);
	}

	public virtual object Deserialize(string value, Type type)
	{
		return JsonConvert.DeserializeObject(value, type, SerializerSettings);
	}

	public virtual void Populate(string value, object target)
	{
		JsonConvert.PopulateObject(value, target, SerializerSettings);
	}

	private JsonSerializerSettings CreateDefaultSettings(JsonSerializerConfiguration configuration)
	{
		return new JsonSerializerSettings
		{
			ContractResolver = new ContractResolver(configuration),
			Converters = new List<JsonConverter>(configuration.Converters),
			SerializationBinder = configuration.SerializationBinder,
			DefaultValueHandling = DefaultValueHandling.Ignore,
			NullValueHandling = NullValueHandling.Ignore,
			TypeNameHandling = TypeNameHandling.Auto,
			Formatting = Formatting.Indented,
			Culture = CultureInfo.InvariantCulture,
			MissingMemberHandling = MissingMemberHandling.Ignore,
			ObjectCreationHandling = ObjectCreationHandling.Replace,
			StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
		};
	}

	private void LoadConverterByType(IEnumerable<JsonConverter> converters)
	{
		foreach (JsonConverter jsonConverter in converters)
		{
			convertByType.TryAdd(jsonConverter.GetType(), jsonConverter);
		}
	}

	public bool TryGetConverter<TConverter>(out TConverter converter) where TConverter : JsonConverter
	{
		if (convertByType.TryGetValue(typeof(TConverter), out var availableConverter))
		{
			converter = (TConverter)availableConverter;
			return true;
		}
		converter = null;
		return false;
	}
}
