public class HUDSidePanelModuleStatShapeCrateSize : HUDSidePanelModuleBaseStat
{
	public readonly int Size;

	public HUDSidePanelModuleStatShapeCrateSize(int size)
	{
		Size = size;
	}

	public override string GetIconId()
	{
		return "stat-shape-crate-size";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.shape-crate-size".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatGenericCount(Size);
	}
}
