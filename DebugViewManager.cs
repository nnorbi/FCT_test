using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebugViewManager
{
	protected List<string> ActiveViews = new List<string>();

	public Dictionary<string, IDebugView> AllViews = new Dictionary<string, IDebugView>();

	public DebugViewManager()
	{
		AddViews();
		ActiveViews = (from name in PlayerPrefs.GetString("debug-views").Split(",")
			where AllViews.ContainsKey(name)
			select name).ToList();
	}

	protected void AddViews()
	{
		AllViews.Add("lanes", new DebugViewLanes());
		AllViews.Add("colliders", new DebugViewColliders());
		AllViews.Add("shapes", new DebugViewShapeBoundaries());
		AllViews.Add("path-placement", new DebugViewPathPlacement());
		AllViews.Add("update-order", new DebugViewUpdateOrder());
		AllViews.Add("fluids", new DebugViewFluidNetwork());
		AllViews.Add("bounds", new DebugViewBounds());
		AllViews.Add("super-chunks", new DebugViewSuperChunks());
		AllViews.Add("lod", new DebugViewLOD());
		AllViews.Add("lod-combined", new DebugViewLODCombined());
		AllViews.Add("shape-asteroids", new DebugViewShapeAsteroids());
	}

	public bool IsActive(string name)
	{
		return ActiveViews.Contains(name);
	}

	public void OnGameDraw()
	{
		foreach (string view in ActiveViews)
		{
			AllViews[view].OnGameDraw();
		}
	}

	public void ShowView(string id)
	{
		if (!ActiveViews.Contains(id))
		{
			ActiveViews.Add(id);
		}
		Save();
	}

	public void HideView(string id)
	{
		if (ActiveViews.Contains(id))
		{
			ActiveViews.Remove(id);
		}
		Save();
	}

	protected void Save()
	{
		PlayerPrefs.SetString("debug-views", string.Join(',', ActiveViews));
		PlayerPrefs.Save();
		Debug.Log("Save player prefs");
	}

	public void HandleInput(InputDownstreamContext inputs)
	{
		foreach (string view in ActiveViews)
		{
			AllViews[view].HandleInput(inputs);
		}
	}
}
