using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[Serializable]
public class EditorClassID<TBaseClass>
{
	[ValidClassId]
	public string ClassID = string.Empty;

	private Type ResolvedType;

	public Type Type
	{
		get
		{
			return ResolvedType ?? (ResolvedType = Resolve());
		}
		private set
		{
			ResolvedType = value;
		}
	}

	public IList<string> AvailableClasses => FindAvailableClassIds();

	public EditorClassID(string defaultValue = "")
	{
		ClassID = defaultValue;
	}

	private IList<string> FindAvailableClassIds()
	{
		return (from t in TypesCacheEditorTime.TypesReflectionForCurrentAppDomain.TypesThatAreAssignableFrom<TBaseClass>()
			where !t.IsAbstract && !t.IsInterface
			select t.FullName).ToArray();
	}

	private Type Resolve()
	{
		Assembly baseAssembly = typeof(TBaseClass).Assembly;
		if (!TypesCacheEditorTime.TypesReflectionForCurrentAppDomain.TryGetTypeByName(baseAssembly, ClassID, out var resolvedType) && !TypesCacheEditorTime.TypesReflectionForCurrentAppDomain.TryGetTypeByName(ClassID, out resolvedType))
		{
			throw new Exception("Could not find ClassID: '" + ClassID + "'.");
		}
		if (!typeof(TBaseClass).IsAssignableFrom(resolvedType))
		{
			throw new Exception("Class '" + ClassID + "' is no implementation of '" + typeof(TBaseClass).Name + "'.");
		}
		return resolvedType;
	}

	public void Validate()
	{
		Type = null;
		Type _ = Type;
	}

	public TBaseClass CreateInstance(params object[] args)
	{
		return (TBaseClass)Activator.CreateInstance(Type, args);
	}
}
