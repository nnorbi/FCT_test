using Unity.Mathematics;

public class HUDSidePanelModuleStatSelectionDimensions : HUDSidePanelModuleBaseStat
{
	public int3 Dimensions { get; protected set; }

	public HUDSidePanelModuleStatSelectionDimensions(TileDimensions dimensions)
	{
		Dimensions = (int3)dimensions;
	}

	public HUDSidePanelModuleStatSelectionDimensions(ChunkDimensions dimensions)
	{
		Dimensions = new int3(dimensions.x, dimensions.y, 0);
	}

	public override string GetIconId()
	{
		return "stat-selection-dimensions";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.selection-dimensions".tr();
	}

	public override string GetContent()
	{
		return (Dimensions.z > 0) ? StringFormatting.FormatDimensions(Dimensions.x, Dimensions.y, Dimensions.z) : StringFormatting.FormatDimensions(Dimensions.x, Dimensions.y);
	}
}
