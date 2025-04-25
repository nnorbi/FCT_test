public class HUDSidePanelModuleStatLitersPerItem : HUDSidePanelModuleBaseStat
{
	public float Liters { get; protected set; }

	public HUDSidePanelModuleStatLitersPerItem(float liters)
	{
		Liters = liters;
	}

	public override string GetIconId()
	{
		return "stat-liters-per-item";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.liters-per-item".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatLiters(Liters);
	}
}
