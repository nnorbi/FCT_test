using System;
using Unity.Mathematics;

public class HUDSidePanelModuleStatProcessingTime : HUDSidePanelModuleBaseStat
{
	public float Duration { get; protected set; }

	public MetaResearchSpeed Speed { get; protected set; }

	public HUDSidePanelModuleStatProcessingTime(float duration, MetaResearchSpeed speed = null)
	{
		Duration = duration;
		Speed = speed;
		if (Speed == null)
		{
			throw new Exception("Speed is null");
		}
	}

	public override string GetIconId()
	{
		return "stat-speed";
	}

	public override string GetTooltipTitle()
	{
		return "side-panel-stat.processing-speed".tr();
	}

	public override string GetTooltipText()
	{
		float itemsPerMinute = 1f / Duration * 60f;
		int currentValue = Singleton<GameCore>.G.Research.SpeedManager.GetSpeedValue(Speed);
		int defaultValue = Singleton<GameCore>.G.Research.Tree.InitialSpeeds[Speed];
		float baseSpeedPerMinute = itemsPerMinute / (float)currentValue * (float)defaultValue;
		float increase = (float)currentValue / (float)defaultValue - 1f;
		return "side-panel-stat.processing-speed.tooltip-base-speed".tr().Replace("<speed>", "<b>" + StringFormatting.FormatShapeAmountThroughputPerMinute(baseSpeedPerMinute) + "</b>") + "<br>" + "side-panel-stat.processing-speed.tooltip-increase".tr().Replace("<increase>", "<b>" + StringFormatting.FormatGeneralPercentage(increase) + "</b>").Replace("<speedName>", Speed.Title);
	}

	public override string GetContent()
	{
		float itemsPerMinute = 1f / Duration * 60f;
		if (!Globals.Settings.Dev.DetailedBuildingEfficiency)
		{
			itemsPerMinute = math.ceil(itemsPerMinute);
		}
		return StringFormatting.FormatShapeAmountThroughputPerMinute(itemsPerMinute);
	}
}
