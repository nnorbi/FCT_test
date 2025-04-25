public class HUDSidePanelModuleStatItemCapacity : HUDSidePanelModuleBaseStat
{
	public int Capacity { get; protected set; }

	public HUDSidePanelModuleStatItemCapacity(int capacity)
	{
		Capacity = capacity;
	}

	public override string GetIconId()
	{
		return "stat-item-capacity";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.item-capacity".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatShapeAmount(Capacity);
	}
}
