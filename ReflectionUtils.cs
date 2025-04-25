using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

[PublicAPI]
public static class ReflectionUtils
{
	private static readonly string[] NonUserAssemblyPrefixes = new string[27]
	{
		"Unity", "UnityEngine", "System", "Mono", "DOTween", "Sirenix", "Newtonsoft", "JetBrains", "HBAO", "ALINE",
		"LeTai", "SingularityGroup", "Bee", "mscorlib", "netstandard", "Tayx", "PPv2", "CodeStage", "LuviKunG", "Autodesk",
		"ShaderGraphVariables", "DemiEditor", "ICSharpCode", "nunit", "Demilib", "Ionic", "RainbowFolders"
	};

	public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type t) => t != null);
		}
	}

	public static IEnumerable<Assembly> GetCurrentDomainUserAssemblies()
	{
		return from asm in AppDomain.CurrentDomain.GetAssemblies()
			where NonUserAssemblyPrefixes.All((string p) => !asm.FullName.StartsWith(p))
			select asm;
	}

	public static IEnumerable<T> CreateInstancesForInterfaceImplementations<T>(this AssemblyTypesCache assemblyTypesCache)
	{
		return assemblyTypesCache.GetConcreteInterfaceImplementations<T>().Select(Activator.CreateInstance).Cast<T>();
	}
}
