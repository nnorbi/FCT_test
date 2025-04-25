using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class HUDSidePanelModuleBuildingEfficiency : HUDSidePanelModule
{
	protected const int MIN_CAPTURE_SIZE = 5;

	protected const int MAX_CAPTURE_SIZE = 20;

	protected BeltLane TargetLane;

	protected BeltLane.PostAcceptHookDelegate SavedPostAcceptHook;

	protected List<ulong> CapturedTimes = new List<ulong>();

	protected float TotalProcessingDuration;

	protected TMP_Text UIDebugText;

	protected TMP_Text UICurrentValueText;

	protected TMP_Text UICurrentPercentageText;

	protected RectTransform UIEfficiencyIndicator;

	protected MapEntity CurrentEntity;

	public HUDSidePanelModuleBuildingEfficiency(MapEntity entity, BeltLane targetLane)
	{
		HUDSidePanelModuleBuildingEfficiency hUDSidePanelModuleBuildingEfficiency = this;
		CurrentEntity = entity;
		TargetLane = targetLane;
		SavedPostAcceptHook = TargetLane.PostAcceptHook;
		targetLane.PostAcceptHook = delegate(BeltLane lane, ref int remainingTicks_T)
		{
			hUDSidePanelModuleBuildingEfficiency.CapturedTimes.Add(entity.Island.Simulator.SimulationTick_I - (ulong)remainingTicks_T);
			if (hUDSidePanelModuleBuildingEfficiency.CapturedTimes.Count > 20)
			{
				hUDSidePanelModuleBuildingEfficiency.CapturedTimes.RemoveAt(0);
			}
			hUDSidePanelModuleBuildingEfficiency.SavedPostAcceptHook?.Invoke(lane, ref remainingTicks_T);
		};
		ComputeTotalProcessingDuration();
	}

	protected void ComputeTotalProcessingDuration()
	{
		MetaBuildingInternalVariant internalVariant = CurrentEntity.InternalVariant;
		HUDSidePanelModuleBaseStat[] stats = internalVariant.HUD_GetStats();
		HUDSidePanelModuleBaseStat speedStat = stats.FirstOrDefault((HUDSidePanelModuleBaseStat stat) => stat is HUDSidePanelModuleStatProcessingTime);
		if (speedStat == null)
		{
			throw new Exception("Efficiency module can't compute processing duration because there is no speed statistic on the building");
		}
		TotalProcessingDuration = ((HUDSidePanelModuleStatProcessingTime)speedStat).Duration;
	}

	public override void Setup(GameObject container)
	{
		base.Setup(container);
		UIDebugText = container.FindText("$DetailsText");
		UIDebugText.text = "";
		UICurrentValueText = container.FindText("$CurrentEfficiencyValue");
		UICurrentValueText.text = "";
		UICurrentPercentageText = container.FindText("$CurrentEfficiencyPercent");
		UICurrentPercentageText.text = "";
		UIEfficiencyIndicator = container.transform.Find("$EfficiencyBar/$EfficiencyIndicator").GetComponent<RectTransform>();
		UIEfficiencyIndicator.gameObject.SetActive(value: false);
		float maxEfficiency = 1f / TotalProcessingDuration * 60f;
		float halfEfficiency = maxEfficiency / 2f;
		container.FindText("$EfficiencyNone").text = StringFormatting.FormatShapeAmountThroughputPerMinute(0f);
		container.FindText("$EfficiencyHalf").text = StringFormatting.FormatShapeAmountThroughputPerMinute(halfEfficiency);
		container.FindText("$EfficiencyFull").text = StringFormatting.FormatShapeAmountThroughputPerMinute(maxEfficiency);
	}

	public override void Cleanup()
	{
		TargetLane.PostAcceptHook = SavedPostAcceptHook;
	}

	protected void UpdateEfficiencyBar(int percent)
	{
		percent = math.clamp(percent, 0, 100);
		int spacing = 20;
		int point0 = 14;
		int point100 = 237;
		int point101 = (point0 + point100) / 2;
		float efficiencyProgress = 0f;
		int scaleFactor = 0;
		if (percent <= 0)
		{
			efficiencyProgress = point0;
			scaleFactor = 1;
		}
		else if (percent > 0 && percent < 50)
		{
			efficiencyProgress = math.lerp(point0 + spacing, point101 - spacing, ((float)percent - 1f) / 48f);
		}
		else if (percent == 50)
		{
			efficiencyProgress = point101;
			scaleFactor = 1;
		}
		else if (percent > 50 && percent < 100)
		{
			efficiencyProgress = math.lerp(point101 + spacing, point100 - spacing, ((float)percent - 51f) / 48f);
		}
		else if (percent >= 100)
		{
			efficiencyProgress = point100;
			scaleFactor = 1;
		}
		DOTween.Kill(UIEfficiencyIndicator);
		UIEfficiencyIndicator.DOAnchorPos(new Vector2(efficiencyProgress, UIEfficiencyIndicator.anchoredPosition.y), 0.12f);
		UIEfficiencyIndicator.DOSizeDelta(math.lerp(new float2(14f, 26f), new float2(29f, 14f), scaleFactor), 0.12f);
	}

	public override void OnGameUpdate(InputDownstreamContext context)
	{
		if (CapturedTimes.Count < 5)
		{
			if (Application.isEditor)
			{
				UIDebugText.text = "Sampling (" + CapturedTimes.Count + " / " + 5 + ")";
			}
			UICurrentValueText.text = StringFormatting.FormatShapeAmountFraction(CapturedTimes.Count, 5);
			UICurrentPercentageText.text = "building-details.efficiency.sampling".tr();
			if (UIEfficiencyIndicator.gameObject.activeSelf)
			{
				UIEfficiencyIndicator.gameObject.SetActive(value: false);
			}
			return;
		}
		int average_T = 0;
		int lowest_T = int.MaxValue;
		int highest_T = int.MinValue;
		for (int i = 0; i < CapturedTimes.Count - 1; i++)
		{
			int delta = (int)(CapturedTimes[i + 1] - CapturedTimes[i]);
			average_T += delta;
			lowest_T = math.min(lowest_T, delta);
			highest_T = math.max(highest_T, delta);
		}
		average_T /= CapturedTimes.Count - 1;
		float averageSeconds = (float)average_T / (float)IslandSimulator.UPS;
		float itemsPerSecond = 1f / averageSeconds;
		float itemsPerMinute = itemsPerSecond * 60f;
		if (!Globals.Settings.Dev.DetailedBuildingEfficiency)
		{
			float maxEfficiency = math.round(1f / TotalProcessingDuration * 60f);
			itemsPerMinute = math.ceil(itemsPerMinute);
			itemsPerMinute = math.clamp(itemsPerMinute, 0f, maxEfficiency);
		}
		UICurrentValueText.text = StringFormatting.FormatShapeAmountThroughputPerMinute(itemsPerMinute);
		float efficiency0To1 = TotalProcessingDuration / averageSeconds;
		int efficiencyPercent = (int)math.ceil(efficiency0To1 * 100f);
		efficiencyPercent = math.clamp(efficiencyPercent, 0, 100);
		if ((bool)Globals.Settings.Dev.DetailedBuildingEfficiency)
		{
			UICurrentPercentageText.text = StringFormatting.FormatEfficiencyPercentage((float)efficiencyPercent / 100f);
		}
		else
		{
			UICurrentPercentageText.text = StringFormatting.FormatEfficiencyPercentage(efficiency0To1);
		}
		if (!UIEfficiencyIndicator.gameObject.activeSelf)
		{
			UIEfficiencyIndicator.gameObject.SetActive(value: true);
		}
		UpdateEfficiencyBar(efficiencyPercent);
		if (Application.isEditor)
		{
			UIDebugText.text = CapturedTimes.Count + "/" + 5 + " items measured\n" + average_T + " ticks / item\n" + (itemsPerSecond * 60f).ToString("F6") + " items / m (" + (itemsPerSecond * 60f).ToString("F2") + " / m)\n" + highest_T + " - " + lowest_T + " = " + (highest_T - lowest_T);
		}
	}
}
