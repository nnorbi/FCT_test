using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class AssemblyTypesCache
{
	private struct AssemblyQualifiedType : IEquatable<AssemblyQualifiedType>
	{
		private Assembly Assembly;

		private readonly string TypeFullname;

		public AssemblyQualifiedType(Assembly assembly, string typeFullname)
		{
			Assembly = assembly;
			TypeFullname = typeFullname;
		}

		public AssemblyQualifiedType(Type type)
			: this(type.Assembly, type.FullName)
		{
		}

		public bool Equals(AssemblyQualifiedType other)
		{
			return object.Equals(Assembly, other.Assembly) && TypeFullname == other.TypeFullname;
		}

		public override bool Equals(object obj)
		{
			return obj is AssemblyQualifiedType other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Assembly, TypeFullname);
		}
	}

	private readonly HashSet<Type> CachedDomainTypes;

	private readonly Dictionary<AssemblyQualifiedType, Type> QualifiedTypeLookup;

	private readonly Dictionary<Type, HashSet<Type>> AssignableFromLookup = new Dictionary<Type, HashSet<Type>>();

	private readonly Assembly[] Assemblies;

	public AssemblyTypesCache(IEnumerable<Assembly> assemblies)
	{
		Assemblies = assemblies.ToArray();
		CachedDomainTypes = Assemblies.SelectMany((Assembly asm) => asm.GetLoadableTypes()).ToHashSet();
		QualifiedTypeLookup = CachedDomainTypes.ToDictionary((Type type) => new AssemblyQualifiedType(type), (Type type) => type);
	}

	public bool TryGetTypeByName(Assembly assembly, string typeFullname, out Type type)
	{
		AssemblyQualifiedType qualifiedType = new AssemblyQualifiedType(assembly, typeFullname);
		return QualifiedTypeLookup.TryGetValue(qualifiedType, out type);
	}

	public bool TryGetTypeByName(string typeFullname, out Type type)
	{
		Assembly[] assemblies = Assemblies;
		foreach (Assembly assembly in assemblies)
		{
			if (TryGetTypeByName(assembly, typeFullname, out type))
			{
				return true;
			}
		}
		type = null;
		return false;
	}

	public IEnumerable<Type> TypesThatAreAssignableFrom<T>()
	{
		return TypesThatAreAssignableFrom(typeof(T));
	}

	public IEnumerable<Type> TypesThatAreAssignableFrom(Type type)
	{
		if (AssignableFromLookup.TryGetValue(type, out var assignableFromSet))
		{
			return assignableFromSet;
		}
		HashSet<Type> set = CachedDomainTypes.Where(type.IsAssignableFrom).ToHashSet();
		AssignableFromLookup.Add(type, set);
		return set;
	}

	public IEnumerable<Type> GetConcreteInterfaceImplementations<T>()
	{
		return from type in TypesThatAreAssignableFrom<T>()
			where !type.IsAbstract
			select type;
	}
}
