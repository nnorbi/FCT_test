using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

internal static class ModsLoader
{
	private const string ASM_EXT = "*.dll";

	public static List<IMod> Mods = new List<IMod>();

	public static bool Patched { get; private set; }

	public static int LoadedModsCount { get; private set; }

	public static void Load()
	{
		SetupPathEnvironmentVariable();
		string persistentPath = Application.persistentDataPath;
		string patchersPath = Path.Combine(persistentPath, GameEnvironmentManager.PATCHERS_PATH);
		string patchersDependenciesPath = Path.Combine(patchersPath, GameEnvironmentManager.PATCHERS_DEPENDENCIES_PATH);
		string modsPath = Path.Combine(persistentPath, GameEnvironmentManager.MODS_PATH);
		if (Directory.Exists(patchersPath) && Directory.Exists(modsPath))
		{
			string[] patchers = Directory.GetFiles(patchersPath, "*.dll", SearchOption.TopDirectoryOnly);
			string[] mods = Directory.GetFiles(modsPath, "*.dll", SearchOption.AllDirectories);
			LoadDependencies(patchersDependenciesPath);
			LoadAssembliesWithBootstrap<IPatcher>(patchers, LoadPatch);
			LoadAssembliesWithBootstrap<IMod>(mods, LoadMod);
		}
	}

	private static void SetupPathEnvironmentVariable()
	{
		string path = Path.Combine(Path.GetFullPath("."), "shapez 2_Data", "Managed");
		Environment.SetEnvironmentVariable("SPZ2_PATH", path);
		Environment.SetEnvironmentVariable("SPZ2_PERSISTENT", Application.persistentDataPath);
	}

	private static void LoadPatch(IPatcher patcher, string path)
	{
		patcher.Patch();
		Patched = true;
	}

	private static void LoadMod(IMod mod, string path)
	{
		mod.Init(Path.GetDirectoryName(path));
		Mods.Add(mod);
		LoadedModsCount++;
	}

	private static void LoadAssembliesWithBootstrap<T>(string[] assemblies, Action<T, string> onLoad)
	{
		foreach (string assemblyPath in assemblies)
		{
			try
			{
				Assembly assembly = Assembly.LoadFrom(assemblyPath);
				Type patchInterfaceType = typeof(T);
				IEnumerable<Type> types = GetLoadableTypes(assembly);
				Type bootstrap = types.SingleOrDefault((Type type) => patchInterfaceType.IsAssignableFrom(type));
				if (bootstrap == null)
				{
					Debug.LogWarning($"Skipping the loading of assembly {assembly} because it could not find any " + "implementation of T");
					break;
				}
				T instance = (T)Activator.CreateInstance(bootstrap);
				onLoad(instance, assemblyPath);
			}
			catch (Exception arg)
			{
				string name = Path.GetFileNameWithoutExtension(assemblyPath);
				Debug.LogError($"Could not load assembly from {name}. Exception: {arg}");
			}
		}
	}

	private static void LoadDependencies(string dependenciesFolder)
	{
		if (Directory.Exists(dependenciesFolder))
		{
			string[] assemblies = Directory.GetFiles(dependenciesFolder, "*.dll", SearchOption.TopDirectoryOnly);
			string[] array = assemblies;
			foreach (string assemblyPath in array)
			{
				Assembly.LoadFrom(assemblyPath);
			}
		}
	}

	private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			Debug.LogWarning(ex.Message);
			return ex.Types.Where((Type t) => t != null);
		}
	}
}
