using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDQuestArea : HUDPart
{
	[SerializeField]
	private HUDPinnedShapes UIPinnedShapes;

	[SerializeField]
	private HUDMilestoneTutorial UIMilestoneTutorial;

	[SerializeField]
	private HUDResearchGoalSuggestions UIGoalSuggestions;

	[SerializeField]
	private RectTransform UILayout;

	[Construct]
	private void Construct()
	{
		AddChildView(UIPinnedShapes);
		AddChildView(UIMilestoneTutorial);
		AddChildView(UIGoalSuggestions);
		UIMilestoneTutorial.LayoutChanged.AddListener(UpdateLayout);
	}

	private void UpdateLayout()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(UILayout);
	}

	protected override void OnDispose()
	{
		UIMilestoneTutorial.LayoutChanged.RemoveListener(UpdateLayout);
	}
}
