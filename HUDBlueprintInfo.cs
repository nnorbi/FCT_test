using Core.Dependency;
using TMPro;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.UI;

public class HUDBlueprintInfo : HUDComponent, IRunnableView, IView
{
	[SerializeField]
	protected HUDIconButton UICurrencyBtn;

	[SerializeField]
	protected TMP_Text UIBlueprintCurrencyValue;

	[SerializeField]
	protected RectTransform UIBlueprintTooltip;

	[SerializeField]
	protected RectTransform UIBlueprintTooltipRatiosParent;

	[SerializeField]
	protected GameObject UIBlueprintTooltipRatioPrefab;

	[SerializeField]
	protected GameObject UIBlueprintTooltipRatioLockedPrefab;

	private ResearchManager ResearchManager;

	private Player Player;

	public void Run()
	{
		Rerender();
		RerenderBlueprintConversionTooltip();
	}

	[Construct]
	private void Construct(ResearchManager researchManager, Player player)
	{
		ResearchManager = researchManager;
		Player = player;
		AddChildView(UICurrencyBtn);
		UICurrencyBtn.Clicked.AddListener(ToggleBlueprintTooltip);
		UIBlueprintTooltip.gameObject.SetActiveSelfExt(active: true);
		ResearchManager.BlueprintCurrencyManager.BlueprintCurrencyChanged.AddListener(OnBlueprintCurrencyChanged);
		ResearchManager.Progress.OnChanged.AddListener(OnProgressChanged);
	}

	protected override void OnDispose()
	{
		UICurrencyBtn.Clicked.RemoveListener(ToggleBlueprintTooltip);
		ResearchManager.BlueprintCurrencyManager.BlueprintCurrencyChanged.RemoveListener(OnBlueprintCurrencyChanged);
		ResearchManager.Progress.OnChanged.RemoveListener(OnProgressChanged);
	}

	protected void OnBlueprintCurrencyChanged(BlueprintCurrency amount)
	{
		Rerender();
	}

	protected void RerenderBlueprintConversionTooltip()
	{
		UIBlueprintTooltipRatiosParent.RemoveAllChildren();
		foreach (GameModeBlueprintCurrencyShape entry in Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintCurrencyShapes)
		{
			bool unlocked = entry.RequiredLevel == null || ResearchManager.Progress.IsUnlocked(entry.RequiredLevel);
			GameObject obj = Object.Instantiate(unlocked ? UIBlueprintTooltipRatioPrefab : UIBlueprintTooltipRatioLockedPrefab, UIBlueprintTooltipRatiosParent);
			obj.FindText("$Ratio").text = StringFormatting.FormatBlueprintCurrency(entry.Amount);
			if (unlocked)
			{
				ShapeDefinition definition = Singleton<GameCore>.G.Shapes.GetDefinitionByHash(entry.Shape);
				HUDBeltItemRenderer.RenderShapeRaw(definition, obj.transform.Find("$ShapeParent"), 40f);
				Button btn = obj.FindButton("$ShapeInfo");
				HUDTheme.PrepareTheme(btn, HUDTheme.ButtonColorsIconOnly).onClick.AddListener(delegate
				{
					Singleton<GameCore>.G.HUD.Events.ShowShapeViewer.Invoke(definition);
				});
			}
			else
			{
				HUDTooltipTarget tooltip = obj.transform.Find("$ShapeParent/$Lock").GetComponent<HUDTooltipTarget>();
				tooltip.Header = "research-info.tooltip-blueprints-ratio-locked.title".tr();
				tooltip.Text = "research-info.tooltip-blueprints-ratio-locked.description".tr();
			}
		}
		UIBlueprintTooltip.SetWidth(20 + (Singleton<GameCore>.G.Mode.ResearchConfig.BlueprintCurrencyShapes.Count * 60 - 10));
	}

	protected void ToggleBlueprintTooltip()
	{
		UIBlueprintTooltip.gameObject.SetActiveSelfExt(!UIBlueprintTooltip.gameObject.activeSelf);
	}

	protected void OnProgressChanged()
	{
		Rerender();
		RerenderBlueprintConversionTooltip();
	}

	protected void Rerender()
	{
		UIBlueprintCurrencyValue.text = StringFormatting.FormatBlueprintCurrency(ResearchManager.BlueprintCurrencyManager.BlueprintCurrency);
		base.gameObject.SetActiveSelfExt(Player.CurrentMap.InteractionMode.AllowBlueprints(Player));
	}
}
