using System;
using System.Collections.Generic;

public class ExpiringResourceManager
{
	protected Dictionary<IExpiringResource, float> ResourcesAndLastUse = new Dictionary<IExpiringResource, float>();

	public int Count => ResourcesAndLastUse.Count;

	public void Register(IExpiringResource resource)
	{
		if (ResourcesAndLastUse.ContainsKey(resource))
		{
			throw new Exception("Can't register " + resource.Name + " twice!");
		}
		ResourcesAndLastUse.Add(resource, -10000f);
	}

	public void Unregister(IExpiringResource resource)
	{
		if (!ResourcesAndLastUse.ContainsKey(resource))
		{
			throw new Exception("Can't unregister " + resource.Name + " - not registered!");
		}
		ResourcesAndLastUse.Remove(resource);
	}

	public void GarbageCollect()
	{
		double now = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
		List<IExpiringResource> keysToDrop = new List<IExpiringResource>();
		foreach (KeyValuePair<IExpiringResource, float> entry in ResourcesAndLastUse)
		{
			if (entry.Key.LastUsed + (double)entry.Key.ExpireAfter < now)
			{
				keysToDrop.Add(entry.Key);
			}
		}
		foreach (IExpiringResource key in keysToDrop)
		{
			key.Hook_OnExpire();
			ResourcesAndLastUse.Remove(key);
		}
	}

	public void OnGameCleanup()
	{
		foreach (KeyValuePair<IExpiringResource, float> item in ResourcesAndLastUse)
		{
			item.Key.Hook_OnExpire();
		}
		ResourcesAndLastUse.Clear();
	}

	public void RegisterCommands(DebugConsole console)
	{
		console.Register("meshcache.stats", delegate(DebugConsole.CommandContext ctx)
		{
			ctx.Output("There are " + ResourcesAndLastUse.Count + " expiring meshes active");
			double currentRealtime = Singleton<GameCore>.G.SimulationSpeed.CurrentRealtime;
			foreach (KeyValuePair<IExpiringResource, float> current in ResourcesAndLastUse)
			{
				ctx.Output(" - " + current.Key.Name + " last: " + (int)current.Key.LastUsed + " (t-" + (int)(currentRealtime - current.Key.LastUsed) + ") expire after " + (int)current.Key.ExpireAfter);
			}
		});
	}
}
