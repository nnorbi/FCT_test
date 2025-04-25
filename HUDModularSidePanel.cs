using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HUDModularSidePanel : HUDBaseSidePanel
{
	[SerializeField]
	public GameObject UIModuleSeparatorPrefab;

	protected HUDSidePanelModule[] CurrentModules;

	public void SetModulesAndContent(string title, IEnumerable<HUDSidePanelModule> modules, IEnumerable<HUDSidePanelHotkeyInfoData> actions)
	{
		CleanupModules();
		ClearContents();
		SetTitle(title);
		CurrentModules = modules.ToArray();
		bool first = true;
		HUDSidePanelModule[] currentModules = CurrentModules;
		foreach (HUDSidePanelModule module in currentModules)
		{
			GameObject prefab = module.GetUIPrefab();
			if ((bool)prefab)
			{
				if (!first)
				{
					Object.Instantiate(UIModuleSeparatorPrefab, base.UIContentContainerTarget);
				}
				else
				{
					first = false;
				}
				GameObject moduleInstance = Object.Instantiate(prefab, base.UIContentContainerTarget);
				module.Setup(moduleInstance);
			}
		}
		SetActions(actions);
	}

	public override void Hide()
	{
		base.Hide();
		CleanupModules();
	}

	protected void CleanupModules()
	{
		if (CurrentModules != null)
		{
			HUDSidePanelModule[] currentModules = CurrentModules;
			foreach (HUDSidePanelModule module in currentModules)
			{
				module.Cleanup();
			}
			CurrentModules = null;
		}
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		CleanupModules();
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		base.OnUpdate(context);
		if (base.Visible && CurrentModules != null)
		{
			HUDSidePanelModule[] currentModules = CurrentModules;
			foreach (HUDSidePanelModule module in currentModules)
			{
				module.OnGameUpdate(context);
			}
		}
	}
}
