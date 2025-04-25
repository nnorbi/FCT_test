using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Global.Core.Serialization.Json;

public class JsonSerializerConfiguration
{
	private MemberTypeSerialization? memberTypeSerialization;

	private HashSet<Type> ignoreTypes;

	private IList<JsonConverter> converters;

	public MemberTypeSerialization MemberTypeSerialization
	{
		get
		{
			return memberTypeSerialization ?? MemberTypeSerialization.Fields;
		}
		set
		{
			memberTypeSerialization = value;
		}
	}

	public HashSet<Type> IgnoreTypes
	{
		get
		{
			return ignoreTypes ?? new HashSet<Type>();
		}
		set
		{
			ignoreTypes = value;
		}
	}

	public IList<JsonConverter> Converters
	{
		get
		{
			return converters ?? new List<JsonConverter>();
		}
		set
		{
			converters = value;
		}
	}

	public ISerializationBinder SerializationBinder { get; set; }

	public bool SerializePrivate { get; set; }
}
