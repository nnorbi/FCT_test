using System;
using System.Collections.Generic;

public class MapManager
{
	protected Dictionary<string, GameMap> Maps = new Dictionary<string, GameMap>();

	public void RegisterMap(GameMap map)
	{
		if (Maps.ContainsKey(map.Id))
		{
			throw new Exception("Map " + map.Id + " is already registered");
		}
		Maps.Add(map.Id, map);
	}

	public GameMap GetMapById(string id)
	{
		return Maps[id];
	}

	public bool HasMap(string id)
	{
		return Maps.ContainsKey(id);
	}

	public void OnGameCleanup()
	{
		foreach (KeyValuePair<string, GameMap> map in Maps)
		{
			map.Value.OnGameCleanup();
		}
		Maps.Clear();
	}

	public IEnumerable<GameMap> GetAllMaps()
	{
		return Maps.Values;
	}

	public void OnGameUpdate(float delta, bool gameIsRendering)
	{
		foreach (KeyValuePair<string, GameMap> map in Maps)
		{
			map.Value.OnGameUpdate(delta, gameIsRendering);
		}
	}
}
