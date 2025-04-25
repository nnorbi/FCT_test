using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;

namespace Global.Core.Serialization.Json;

public class ExplicitTypeBinder : ISerializationBinder
{
	private readonly Dictionary<string, Type> TypeByName = new Dictionary<string, Type>();

	private readonly Dictionary<Type, string> NameByType = new Dictionary<Type, string>();

	public ExplicitTypeBinder(params (string, Type)[] bindings)
		: this((IEnumerable<(string, Type)>)bindings)
	{
	}

	public ExplicitTypeBinder(IEnumerable<(string, Type)> bindings)
	{
		foreach (var binding in bindings)
		{
			TypeByName.Add(binding.Item1, binding.Item2);
			NameByType.Add(binding.Item2, binding.Item1);
		}
	}

	public Type BindToType(string assemblyName, string typeName)
	{
		return TypeByName[typeName];
	}

	public void BindToName(Type type, out string assemblyName, out string typeName)
	{
		assemblyName = null;
		typeName = NameByType[type];
	}
}
