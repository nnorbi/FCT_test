public class HUDSidePanelModuleStatFluidProduction : HUDSidePanelModuleBaseStat
{
	public float LitersPerMinute { get; protected set; }

	public HUDSidePanelModuleStatFluidProduction(float litersPerMinute)
	{
		LitersPerMinute = litersPerMinute;
	}

	public override string GetIconId()
	{
		return "stat-fluid-production";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.fluid-production".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatLitersFlowPerMinuteSigned(LitersPerMinute);
	}
}
