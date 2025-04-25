public class HUDSidePanelModuleStatBuildingCount : HUDSidePanelModuleBaseStat
{
	public int Count { get; protected set; }

	public int Discount { get; protected set; }

	public HUDSidePanelModuleStatBuildingCount(int count, int discount)
	{
		Count = count;
		Discount = discount;
	}

	public override string GetIconId()
	{
		return "stat-building-count";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.building-count".tr();
	}

	public override string GetContent()
	{
		return (Discount > 0) ? "side-panel-stat.blueprint-cost.text-with-discount".tr(("<building-count>", StringFormatting.FormatGenericCount(Count)), ("<discount>", StringFormatting.FormatGenericCount(Discount))) : "side-panel-stat.blueprint-cost.text".tr(("<building-count>", StringFormatting.FormatGenericCount(Count)));
	}
}
