using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HUDSidePanelModuleStats : HUDSidePanelModule
{
	protected HUDSidePanelModuleBaseStat[] Stats;

	public HUDSidePanelModuleStats(IEnumerable<HUDSidePanelModuleBaseStat> stats)
	{
		Stats = stats.ToArray();
	}

	public override void Setup(GameObject container)
	{
		RectTransform rect = container.GetComponent<RectTransform>();
		HUDSidePanelModuleStatsExtraData extraData = container.GetComponent<HUDSidePanelModuleStatsExtraData>();
		HUDSidePanelModuleBaseStat[] stats = Stats;
		foreach (HUDSidePanelModuleBaseStat stat in stats)
		{
			GameObject obj = Object.Instantiate(extraData.UIDetailPanelStatsPrefab, rect);
			Image icon = obj.FindImage("$Icon");
			icon.sprite = Globals.Resources.UIGlobalIconMapping.Get(stat.GetIconId());
			obj.FindText("$Label").text = stat.GetContent();
			HUDTooltipTarget tooltip = icon.GetComponent<HUDTooltipTarget>();
			tooltip.TranslateTexts = false;
			tooltip.Header = stat.GetTooltipTitle();
			tooltip.Text = stat.GetTooltipText();
			obj.FindImage("$InfoIcon").gameObject.SetActiveSelfExt(!string.IsNullOrEmpty(tooltip.Header));
		}
		rect.SetHeight(Stats.Length * 40 - 10);
	}
}
