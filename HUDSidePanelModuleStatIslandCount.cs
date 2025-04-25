public class HUDSidePanelModuleStatIslandCount : HUDSidePanelModuleBaseStat
{
	public int Count { get; protected set; }

	public HUDSidePanelModuleStatIslandCount(int count)
	{
		Count = count;
	}

	public override string GetIconId()
	{
		return "stat-island-count";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.island-count".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatGenericCount(Count);
	}
}
