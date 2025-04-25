public class HUDSidePanelModuleStatChunkCount : HUDSidePanelModuleBaseStat
{
	public int Count { get; protected set; }

	public HUDSidePanelModuleStatChunkCount(int count)
	{
		Count = count;
	}

	public override string GetIconId()
	{
		return "stat-chunk-count";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.chunk-count".tr();
	}

	public override string GetContent()
	{
		return StringFormatting.FormatGenericCount(Count);
	}
}
