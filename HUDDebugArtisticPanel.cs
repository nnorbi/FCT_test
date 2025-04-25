using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using UnityEngine;

public class HUDDebugArtisticPanel : HUDPart
{
	[SerializeField]
	protected HUDDebugArtisticPanelTabButton[] UITabs;

	protected bool Visible;

	public override bool ShouldInitialize => false;

	[Construct]
	private IEnumerable<HUDDebugArtisticPanelTabButton> Construct()
	{
		base.gameObject.SetActive(value: false);
		HUDDebugArtisticPanelTabButton[] uITabs = UITabs;
		foreach (HUDDebugArtisticPanelTabButton tab in uITabs)
		{
			tab.OnClicked.AddListener(delegate
			{
				SwitchToTab(tab);
			});
		}
		SwitchToTab(UITabs.First());
		return UITabs;
	}

	protected override void OnDispose()
	{
		HUDDebugArtisticPanelTabButton[] uITabs = UITabs;
		foreach (HUDDebugArtisticPanelTabButton tab in uITabs)
		{
			tab.Dispose();
		}
	}

	private void SwitchToTab(HUDDebugArtisticPanelTabButton activeTab)
	{
		HUDDebugArtisticPanelTabButton[] uITabs = UITabs;
		foreach (HUDDebugArtisticPanelTabButton tab in uITabs)
		{
			tab.Hide();
		}
		activeTab.Show();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (context.ConsumeWasActivated("debug.show-artistic-panel"))
		{
			Visible = !Visible;
			base.gameObject.SetActive(Visible);
		}
	}
}
