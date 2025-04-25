using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Global.Core.Serialization.Json;

internal class ContractResolver : DefaultContractResolver
{
	private readonly JsonSerializerConfiguration configuration;

	public ContractResolver(JsonSerializerConfiguration configuration)
	{
		this.configuration = configuration;
	}

	protected override List<MemberInfo> GetSerializableMembers(Type objectType)
	{
		return SearchMembers(objectType);
	}

	private List<MemberInfo> SearchMembers(Type objectType)
	{
		List<MemberInfo> result = new List<MemberInfo>();
		SearchPublicMembers(objectType, result);
		if (configuration.SerializePrivate)
		{
			SearchPrivateMembers(objectType, result);
		}
		return result;
	}

	private void SearchPublicMembers(Type objectType, List<MemberInfo> list)
	{
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
		list.AddRange(objectType.GetMembers(bindingFlags).Where(MemberCanBeSerialized));
	}

	private void SearchPrivateMembers(Type objectType, List<MemberInfo> list)
	{
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
		list.AddRange(objectType.GetMembers(bindingFlags).Where(MemberCanBeSerialized));
		if (objectType.BaseType != null)
		{
			SearchPrivateMembers(objectType.BaseType, list);
		}
	}

	private bool MemberCanBeSerialized(MemberInfo memberInfo)
	{
		if (1 == 0)
		{
		}
		bool result = ((memberInfo is PropertyInfo propertyInfo) ? (!TypeIsIgnored(propertyInfo.PropertyType) && MemberTypeIsEnabled(MemberTypeSerialization.Properties)) : (memberInfo is FieldInfo fieldInfo && !TypeIsIgnored(fieldInfo.FieldType) && MemberTypeIsEnabled(MemberTypeSerialization.Fields) && FieldIsNotCompilerGenerated(fieldInfo)));
		if (1 == 0)
		{
		}
		return result;
	}

	private bool TypeIsIgnored(Type type)
	{
		if (configuration.IgnoreTypes == null)
		{
			return false;
		}
		return configuration.IgnoreTypes.Contains(type);
	}

	private bool MemberTypeIsEnabled(MemberTypeSerialization memberType)
	{
		return configuration.MemberTypeSerialization.HasFlag(memberType);
	}

	private bool FieldIsNotCompilerGenerated(FieldInfo fieldInfo)
	{
		return !fieldInfo.IsDefined(typeof(CompilerGeneratedAttribute));
	}

	protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
	{
		return base.CreateProperty(member, MemberSerialization.Fields);
	}
}
