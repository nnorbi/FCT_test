using System.Collections.Generic;
using Core.Dependency;
using Unity.Core.View;
using UnityEngine;

public abstract class HUDPartWithSidePanel : HUDPart
{
	[SerializeField]
	private PrefabViewReference<HUDModularSidePanel> UIModularSidePanelPrefab;

	private bool SidePanelNeedsRerender = false;

	protected HUDModularSidePanel SidePanel;

	private ResearchManager ResearchManager;

	[Construct]
	private void Construct(ResearchManager researchManager)
	{
		ResearchManager = researchManager;
		SidePanel = RequestChildView(UIModularSidePanelPrefab).PlaceAt(base.transform);
		ResearchManager.Progress.OnChanged.AddListener(SidePanel_MarkDirty);
	}

	protected override void OnDispose()
	{
		ResearchManager.Progress.OnChanged.RemoveListener(SidePanel_MarkDirty);
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		bool shouldBeVisible = context.IsTokenAvailable("HUDPart$main_interaction") && SidePanel_ShouldShow();
		if (!shouldBeVisible)
		{
			SidePanel.Hide();
		}
		else if (!SidePanel.Visible)
		{
			SidePanel_RerenderInternal();
			SidePanelNeedsRerender = false;
			SidePanel.Show();
		}
		if (SidePanelNeedsRerender)
		{
			SidePanelNeedsRerender = false;
			if (shouldBeVisible)
			{
				SidePanel_RerenderInternal();
			}
		}
		base.OnGameUpdate(context, drawOptions);
	}

	private void SidePanel_RerenderInternal()
	{
		SidePanel.SetModulesAndContent(SidePanel_GetTitle(), SidePanel_GetModules(), SidePanel_GetActions());
	}

	protected abstract bool SidePanel_ShouldShow();

	protected abstract IEnumerable<HUDSidePanelModule> SidePanel_GetModules();

	protected abstract IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions();

	protected abstract string SidePanel_GetTitle();

	protected void SidePanel_MarkDirty()
	{
		SidePanelNeedsRerender = true;
	}
}
