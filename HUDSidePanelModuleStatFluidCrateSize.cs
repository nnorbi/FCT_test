public class HUDSidePanelModuleStatFluidCrateSize : HUDSidePanelModuleBaseStat
{
	public readonly float Liters;

	public HUDSidePanelModuleStatFluidCrateSize(float liters)
	{
		Liters = liters;
	}

	public override string GetIconId()
	{
		return "stat-fluid-crate-size";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.fluid-crate-size".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatLiters(Liters);
	}
}
