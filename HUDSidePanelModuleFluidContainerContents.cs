using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

public class HUDSidePanelModuleFluidContainerContents : HUDSidePanelModule
{
	protected struct TimeSample
	{
		public double Time_I;

		public float Value;
	}

	protected ProceduralImage UILevelContainerImage;

	protected Image UIFluidIcon;

	protected TMP_Text UICurrentValueText;

	protected TMP_Text UICurrentFlowText;

	protected TMP_Text UILevelDeltaText;

	protected FluidContainer Container;

	protected Fluid LastFluid = null;

	protected float FlowMax = 0f;

	protected List<TimeSample> ValueHistory = new List<TimeSample>();

	public HUDSidePanelModuleFluidContainerContents(FluidContainer container, float passiveFlow = 0f)
	{
		Container = container;
		FlowMax = passiveFlow;
	}

	public override void Setup(GameObject contentsContainer)
	{
		base.Setup(contentsContainer);
		ValueHistory.Clear();
		ValueHistory.Add(new TimeSample
		{
			Time_I = Container.Entity.Island.Simulator.SimulationTime_I,
			Value = Container.Value
		});
		UILevelContainerImage = contentsContainer.transform.Find("$FillIndicator/$Value").GetComponent<ProceduralImage>();
		UIFluidIcon = contentsContainer.transform.Find("$ContentsBg/$FluidIndicator").GetComponent<Image>();
		UICurrentValueText = contentsContainer.FindText("$CurrentValueText");
		UICurrentFlowText = contentsContainer.FindText("$CurrentFlowText");
		UILevelDeltaText = contentsContainer.FindText("$LevelDeltaText");
		contentsContainer.FindText("$FillMaxText").text = StringFormatting.FormatLiters(Container.Max);
		contentsContainer.FindText("$FlowMaxText").text = StringFormatting.FormatLitersFlowPerMinuteSigned(FlowMax * 60f);
		SetEmptyContainerUIState();
		Button flushAllBtn = contentsContainer.FindButton("$FlushAllBtn");
		HUDTheme.PrepareTheme(flushAllBtn, HUDTheme.ButtonColorsDanger).onClick.AddListener(delegate
		{
			Container.Network.FlushAll();
		});
	}

	public void TryFlush()
	{
		Container.Flush();
	}

	protected void SetEmptyContainerUIState()
	{
		UIFluidIcon.gameObject.SetActive(value: false);
		UICurrentValueText.text = StringFormatting.FormatLiters(0f);
		UILevelDeltaText.text = StringFormatting.FormatLitersFlowPerMinuteSigned(0f);
		UICurrentFlowText.text = StringFormatting.FormatLitersFlowPerMinuteSigned(0f);
		UILevelContainerImage.color = new Color(0f, 0f, 0f, 0f);
		UILevelContainerImage.fillAmount = 0f;
	}

	protected float ComputeAverageValueDeltaPerSecond()
	{
		float deltas = 0f;
		if (ValueHistory.Count < 2)
		{
			return 0f;
		}
		for (int i = 0; i < ValueHistory.Count - 1; i++)
		{
			TimeSample value0 = ValueHistory[i];
			TimeSample value1 = ValueHistory[i + 1];
			float flowPerSecond = (value1.Value - value0.Value) / (float)math.max(0.001, value1.Time_I - value0.Time_I);
			deltas += flowPerSecond;
		}
		return deltas / ((float)ValueHistory.Count - 1f);
	}

	public override void OnGameUpdate(InputDownstreamContext context)
	{
		ValueHistory.Add(new TimeSample
		{
			Time_I = Container.Entity.Island.Simulator.SimulationTime_I,
			Value = Container.Value
		});
		while (ValueHistory.Count > 20)
		{
			ValueHistory.RemoveAt(0);
		}
		if (Container.Fluid != LastFluid)
		{
			LastFluid = Container.Fluid;
			if (LastFluid == null)
			{
				SetEmptyContainerUIState();
			}
			else
			{
				UIFluidIcon.gameObject.SetActive(value: true);
				UIFluidIcon.color = LastFluid.GetUIColor();
			}
		}
		if (Container.Fluid != null)
		{
			UILevelContainerImage.fillAmount = math.saturate(Container.Level);
			UILevelContainerImage.color = Container.Fluid.GetUIColor();
			UICurrentValueText.text = StringFormatting.FormatLiters(Container.Value);
			if (ValueHistory.Count > 3 && ValueHistory[ValueHistory.Count - 1].Time_I - ValueHistory[0].Time_I > 0.5)
			{
				UILevelDeltaText.text = StringFormatting.FormatLitersFlowPerMinuteSigned(ComputeAverageValueDeltaPerSecond() * 60f);
			}
			else
			{
				UILevelDeltaText.text = "-";
			}
			UICurrentFlowText.text = StringFormatting.FormatLitersFlowPerMinuteSigned(Container.ComputeTotalFlow() * 60f);
		}
	}
}
