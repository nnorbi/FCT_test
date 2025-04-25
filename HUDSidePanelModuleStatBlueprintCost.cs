public class HUDSidePanelModuleStatBlueprintCost : HUDSidePanelModuleBaseStat
{
	public BlueprintCurrency Cost { get; protected set; }

	public HUDSidePanelModuleStatBlueprintCost(BlueprintCurrency cost)
	{
		Cost = cost;
	}

	public override string GetIconId()
	{
		return "stat-blueprint-cost";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.blueprint-cost".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatBlueprintCurrency(Cost);
	}
}
