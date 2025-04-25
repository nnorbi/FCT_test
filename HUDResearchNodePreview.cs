using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Collections.Scoped;
using Core.Dependency;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class HUDResearchNodePreview : HUDComponent
{
	[SerializeField]
	private HUDResearchNodeProgress UINodeProgress;

	[SerializeField]
	private HUDShapeDisplay UIShapeCostDisplay;

	[SerializeField]
	private TMP_Text UIResearchTitleText;

	[SerializeField]
	private CanvasGroup UIMainCanvasGroup;

	[SerializeField]
	private GameObject UIBackgroundTranslucentBg;

	[SerializeField]
	private GameObject UIGoalCompletedIndicator;

	[SerializeField]
	private GameObject UIPinIndicator;

	[SerializeField]
	private HUDPrimaryButtonPanel UIButton;

	[SerializeField]
	private HUDTooltipTarget Tooltip;

	[NonSerialized]
	public UnityEvent Clicked = new UnityEvent();

	private ResearchManager ResearchManager;

	private ShapeManager ShapeManager;

	private Player Player;

	private GameModeHandle GameMode;

	private IResearchableHandle _Research;

	private bool _TranslucentBackground = true;

	public bool TranslucentBackground
	{
		set
		{
			_TranslucentBackground = value;
		}
	}

	public IResearchableHandle Research
	{
		get
		{
			return _Research;
		}
		set
		{
			if (value != _Research)
			{
				_Research = value;
				UpdateView();
			}
		}
	}

	public bool ShowPin { get; set; }

	public bool ShowTooltip
	{
		set
		{
			Tooltip.enabled = value;
		}
	}

	[Construct]
	private void Construct(ResearchManager researchManager, ShapeManager shapeManager, Player player, GameModeHandle gameMode)
	{
		ResearchManager = researchManager;
		ShapeManager = shapeManager;
		Player = player;
		GameMode = gameMode;
		AddChildView(UINodeProgress);
		AddChildView(UIShapeCostDisplay);
		AddChildView(UIButton);
		UIButton.OnClicked.AddListener(HandleMainButtonClick);
	}

	protected override void OnDispose()
	{
		UIButton.OnClicked.RemoveListener(HandleMainButtonClick);
		Clicked.RemoveAllListeners();
	}

	private void UpdateView()
	{
		UINodeProgress.Research = Research;
		UIShapeCostDisplay.Shape = ((Research != null) ? ShapeManager.GetDefinitionByHash(Research.Cost.DefinitionHash) : null);
		UpdateTooltip();
	}

	protected void UpdateTooltip()
	{
		if (Research == null)
		{
			return;
		}
		MetaResearchable researchable = Research.Meta;
		UIResearchTitleText.text = researchable.Title;
		Tooltip.Header = researchable.Title;
		string tooltipText = researchable.Description;
		if (researchable.SpeedAdjustments.Count > 0)
		{
			using ScopedHashSet<(MetaBuildingVariant, MetaResearchable.SpeedAdjustmentData)> affectedBuildings = ScopedHashSet<(MetaBuildingVariant, MetaResearchable.SpeedAdjustmentData)>.Get();
			foreach (MetaResearchable.SpeedAdjustmentData speedOverride in researchable.SpeedAdjustments)
			{
				foreach (MetaBuildingVariant buildingVariant in FindEffectedBuildings(speedOverride.Speed))
				{
					affectedBuildings.Add((buildingVariant, speedOverride));
				}
			}
			tooltipText += "\n";
			foreach (var item in affectedBuildings)
			{
				var (buildingVariant2, effect) = item;
				if (ResearchManager.Progress.UnlockedResearchables.FirstOrDefault((MetaResearchable r) => r.Unlocks.Contains(buildingVariant2)) != null)
				{
					int initialSpeed = ResearchManager.Tree.InitialSpeeds[effect.Speed];
					int currentSpeed = ResearchManager.SpeedManager.GetSpeedValue(effect.Speed);
					int deltaSpeed = effect.AdditiveSpeed * initialSpeed / 100;
					int newSpeed = currentSpeed + deltaSpeed;
					float processingDuration = GetProcessingDuration(buildingVariant2.InternalVariants[0]);
					float currentThroughput = (float)currentSpeed / processingDuration * 60f / 100f;
					float newThroughput = (float)newSpeed / processingDuration * 60f / 100f;
					string title = (buildingVariant2.Building.GroupVariantsInSpeedTooltip ? buildingVariant2.Building.Title : buildingVariant2.Title);
					tooltipText += "\n\n";
					tooltipText += "research-info.tooltip-speed-update-detail".tr(("<building>", title), ("<oldSpeed>", StringFormatting.FormatShapeAmountThroughputPerMinuteRaw(currentThroughput)), ("<newSpeed>", StringFormatting.FormatShapeAmountThroughputPerMinuteRaw(newThroughput)));
				}
			}
		}
		if (researchable.BlueprintDiscount != 0)
		{
			tooltipText += "\n\n";
			tooltipText += "research-info.tooltip-blueprint-discount".tr(("<discount>", StringFormatting.FormatGenericCount(researchable.BlueprintDiscount)));
		}
		foreach (KeyValuePair<MetaResearchable, int> limitIncrease in GameMode.ResearchConfig.ChunkLimitUnlocks)
		{
			if (limitIncrease.Key == researchable)
			{
				tooltipText += "\n\n";
				tooltipText += "research-info.tooltip-platform-limit".tr(("<increase>", StringFormatting.FormatGenericCount(limitIncrease.Value)));
			}
		}
		Tooltip.Text = tooltipText;
	}

	protected void HandleMainButtonClick()
	{
		if (ResearchManager.Progress.IsUnlocked(Research))
		{
			Globals.UISounds.PlayError();
		}
		else
		{
			Clicked.Invoke();
		}
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (Research != null)
		{
			bool isUnlocked = ResearchManager.Progress.IsUnlocked(Research);
			UIMainCanvasGroup.alpha = (isUnlocked ? 0.25f : 1f);
			UIBackgroundTranslucentBg.SetActiveSelfExt(!isUnlocked && _TranslucentBackground);
			UIGoalCompletedIndicator.SetActiveSelfExt(isUnlocked);
			UIPinIndicator.SetActiveSelfExt(ShowPin && Player.Pins.IsPinned(Research));
			bool isUnlockable = !isUnlocked && ResearchManager.CanUnlock(Research);
			UIButton.Active = isUnlockable;
			UIShapeCostDisplay.Interactable = !ResearchManager.CanUnlock(Research);
		}
	}

	private IEnumerable<MetaBuildingVariant> FindEffectedBuildings(MetaResearchSpeed researchSpeed)
	{
		foreach (MetaBuilding building in Singleton<GameCore>.G.Mode.Buildings)
		{
			foreach (MetaBuildingVariant buildingVariant in building.Variants)
			{
				if (buildingVariant.ShowInToolbar && buildingVariant.PlayerBuildable && IsEffectedByResearchSpeed(buildingVariant, researchSpeed))
				{
					yield return buildingVariant;
					if (building.GroupVariantsInSpeedTooltip)
					{
						break;
					}
				}
			}
		}
	}

	private static bool IsEffectedByResearchSpeed(MetaBuildingVariant buildingVariant, MetaResearchSpeed researchSpeed)
	{
		MetaBuildingInternalVariant[] internalVariants = buildingVariant.InternalVariants;
		foreach (MetaBuildingInternalVariant internalVariant in internalVariants)
		{
			BeltLaneDefinition[] beltLaneDefinitions = internalVariant.BeltLaneDefinitions;
			foreach (BeltLaneDefinition laneDefinition in beltLaneDefinitions)
			{
				if (laneDefinition.Speed == researchSpeed)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static float GetProcessingDuration(MetaBuildingInternalVariant internalVariant)
	{
		MethodInfo statMethod = internalVariant.Implementation.Type.GetMethod("HUD_GetProcessingDurationRaw", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
		return (float)statMethod.Invoke(null, new object[1] { internalVariant });
	}
}
