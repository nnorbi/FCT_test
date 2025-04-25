using System.Collections.Generic;
using Core.Dependency;
using UnityEngine;

public class HUDDebugPanelTabDebugViews : HUDDebugPanelTab
{
	[SerializeField]
	protected HUDDebugViewEntry UIHUDDebugViewPrefab;

	[SerializeField]
	protected RectTransform UIDebugViewsParent;

	protected DebugViewManager DebugViewManager;

	[Construct]
	private void Construct(DebugViewManager debugViewManager)
	{
		DebugViewManager = debugViewManager;
		UIDebugViewsParent.RemoveAllChildren();
		foreach (KeyValuePair<string, IDebugView> allView in DebugViewManager.AllViews)
		{
			allView.Deconstruct(out var key, out var value);
			string viewId = key;
			IDebugView view = value;
			HUDDebugViewEntry viewEntryInstance = Object.Instantiate(UIHUDDebugViewPrefab, UIDebugViewsParent);
			viewEntryInstance.Setup(view.Name, DebugViewManager.IsActive(viewId), delegate(bool active)
			{
				OnToggleChange(viewId, active);
			});
		}
	}

	protected void OnToggleChange(string id, bool active)
	{
		if (active)
		{
			DebugViewManager.ShowView(id);
		}
		else
		{
			DebugViewManager.HideView(id);
		}
	}
}
