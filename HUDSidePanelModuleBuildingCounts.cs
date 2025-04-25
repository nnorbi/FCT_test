using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDSidePanelModuleBuildingCounts : HUDSidePanelModule
{
	public delegate void OnNarrowDownDelegate(MetaBuilding building, bool selectAllButType);

	public OnNarrowDownDelegate OnNarrowDown;

	protected (MetaBuilding, int)[] Counts;

	public HUDSidePanelModuleBuildingCounts(IEnumerable<(MetaBuilding, int)> counts, OnNarrowDownDelegate onNarrowDown)
	{
		Counts = counts.ToArray();
		OnNarrowDown = onNarrowDown;
	}

	public override void Setup(GameObject container)
	{
		base.Setup(container);
		int buildingsPerRow = 5;
		int buildingIconEffectiveSize = 65;
		RectTransform contentTransform = ContentContainer.GetComponent<RectTransform>();
		contentTransform.sizeDelta = new Vector2(contentTransform.sizeDelta.x, (int)math.ceil((float)Counts.Length / (float)buildingsPerRow) * buildingIconEffectiveSize - 5);
		HUDSidePanelModuleBuildingCountsExtraData extraData = ContentContainer.GetComponent<HUDSidePanelModuleBuildingCountsExtraData>();
		for (int i = 0; i < Counts.Length; i++)
		{
			(MetaBuilding, int) entry = Counts[i];
			GameObject obj = Object.Instantiate(extraData.UIBuildingIconAndCountPrefab, contentTransform);
			Image icon = obj.FindImage("$Icon");
			TMP_Text countText = obj.FindText("$Count");
			icon.sprite = entry.Item1.Icon;
			icon.material = Globals.Resources.DefaultUISpriteMaterial;
			countText.text = StringFormatting.FormatGenericCount(entry.Item2);
			HUDTooltipTarget tooltip = icon.GetComponent<HUDTooltipTarget>();
			tooltip.Header = entry.Item1.Title;
			tooltip.Text = ((OnNarrowDown != null) ? "selection-details.narrow-down.description".tr() : string.Empty);
			Button btn = obj.GetComponent<Button>();
			HUDTheme.PrepareTheme(btn, HUDTheme.ButtonColorsIconOnly).onClick.AddListener(delegate
			{
				OnNarrowDown?.Invoke(entry.Item1, !Globals.Keybindings.GetById("mass-selection.select-all-of-building-type-modifier").IsAnySetActive());
			});
		}
	}
}
