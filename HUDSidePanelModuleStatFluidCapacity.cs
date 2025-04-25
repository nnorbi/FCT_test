public class HUDSidePanelModuleStatFluidCapacity : HUDSidePanelModuleBaseStat
{
	public float Capacity { get; protected set; }

	public HUDSidePanelModuleStatFluidCapacity(float capacity)
	{
		Capacity = capacity;
	}

	public override string GetIconId()
	{
		return "stat-fluid-capacity";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.fluid-capacity".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatLiters(Capacity);
	}
}
