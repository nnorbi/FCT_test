using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using UnityEngine;

public class HUDDebugPanel : HUDPart
{
	[SerializeField]
	protected HUDDebugTab[] UITabs;

	private bool Visible;

	private HUDDebugTab ActiveTab;

	public override bool ShouldInitialize => false;

	[Construct]
	private IEnumerable<HUDDebugPanelTab> Construct()
	{
		base.gameObject.SetActive(value: false);
		HUDDebugTab[] uITabs = UITabs;
		for (int i = 0; i < uITabs.Length; i++)
		{
			HUDDebugTab tab = uITabs[i];
			tab.Button.OnClicked.AddListener(delegate
			{
				SwitchToTab(tab);
			});
		}
		SwitchToTab(UITabs.First());
		return UITabs.Select((HUDDebugTab x) => x.Content);
	}

	protected override void OnDispose()
	{
		HUDDebugTab[] uITabs = UITabs;
		foreach (HUDDebugTab tab in uITabs)
		{
			tab.Dispose();
		}
	}

	private void SwitchToTab(HUDDebugTab activeTab)
	{
		HUDDebugTab[] uITabs = UITabs;
		foreach (HUDDebugTab tab in uITabs)
		{
			tab.Hide();
		}
		ActiveTab = activeTab;
		activeTab.Show();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (context.ConsumeWasActivated("debug.show-artistic-panel"))
		{
			Visible = !Visible;
			base.gameObject.SetActive(Visible);
		}
		if (ActiveTab.Content is IHUDDebugUpdateable updateable)
		{
			updateable.OnUpdate(context, drawOptions);
		}
	}
}
