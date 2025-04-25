using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDResearchGoalSuggestions : HUDComponent
{
	private const int MAX_SUGGESTIONS = 3;

	[SerializeField]
	protected PrefabViewReference<HUDResearchNodePreview> ResearchNodePreviewPrefab;

	[SerializeField]
	protected RectTransform UIItemsParent;

	protected List<HUDResearchNodePreview> SuggestionInstances = new List<HUDResearchNodePreview>();

	private Player Player;

	private ResearchManager ResearchManager;

	[Construct]
	private void Construct(Player player, ResearchManager researchManager)
	{
		Player = player;
		ResearchManager = researchManager;
		Player.Pins.OnChanged.AddListener(RebuildSuggestions);
		ResearchManager.Progress.OnChanged.AddListener(RebuildSuggestions);
		RebuildSuggestions();
		UpdateLayout();
	}

	protected override void OnDispose()
	{
		Player.Pins.OnChanged.RemoveListener(RebuildSuggestions);
		ResearchManager.Progress.OnChanged.RemoveListener(RebuildSuggestions);
	}

	private void RemoveSuggestion(HUDResearchNodePreview suggestion)
	{
		if (!SuggestionInstances.Remove(suggestion))
		{
			base.Logger.Warning?.Log("Failed to remove suggestion, not contained");
		}
		ReleaseChildView(suggestion);
	}

	protected void RebuildSuggestions()
	{
		bool layoutChanged = false;
		int pinCount = Player.Pins.PinnedResearchables.Count;
		int maxSuggestions = math.min(math.max(0, 7 - pinCount), 3);
		foreach (HUDResearchNodePreview suggestion in SuggestionInstances.ToList())
		{
			if (!IsValidSuggestion(suggestion.Research))
			{
				RemoveSuggestion(suggestion);
				layoutChanged = true;
			}
		}
		while (SuggestionInstances.Count > maxSuggestions)
		{
			List<HUDResearchNodePreview> suggestionInstances = SuggestionInstances;
			HUDResearchNodePreview last = suggestionInstances[suggestionInstances.Count - 1];
			RemoveSuggestion(last);
			layoutChanged = true;
		}
		ResearchSideGoalHandle[] sideGoals = ResearchManager.Tree.SideGoals;
		foreach (ResearchSideGoalHandle node in sideGoals)
		{
			if (SuggestionInstances.Count >= maxSuggestions)
			{
				break;
			}
			if (IsValidSuggestion(node) && !SuggestionInstances.Any((HUDResearchNodePreview n) => n.Research == node))
			{
				HUDResearchNodePreview researchNodePreview = RequestChildView(ResearchNodePreviewPrefab).PlaceAt(UIItemsParent);
				researchNodePreview.ShowTooltip = true;
				researchNodePreview.ShowPin = false;
				researchNodePreview.Research = node;
				SuggestionInstances.Add(researchNodePreview);
				researchNodePreview.Clicked.AddListener(delegate
				{
					OnSuggestionClicked(node);
				});
				layoutChanged = true;
			}
		}
		if (layoutChanged)
		{
			UpdateLayout();
		}
	}

	protected void OnSuggestionClicked(IResearchableHandle node)
	{
		if (ResearchManager.CanUnlock(node))
		{
			if (ResearchManager.TryUnlock(node))
			{
				Globals.UISounds.PlayClick();
			}
			else
			{
				Globals.UISounds.PlayError();
			}
		}
		else if (!Player.Pins.TryPin(node))
		{
			Debug.LogError("Failed to pin suggestion");
		}
	}

	protected bool IsValidSuggestion(IResearchableHandle node)
	{
		return ResearchManager.CanReach(node) && !ResearchManager.Progress.IsUnlocked(node) && !Player.Pins.IsPinned(node);
	}

	protected void UpdateLayout()
	{
		float padding = UIItemsParent.GetComponent<VerticalLayoutGroup>().spacing;
		float height = ResearchNodePreviewPrefab.Resolve().GetComponent<RectTransform>().sizeDelta.y;
		float headerHeight = 20f + padding;
		UIItemsParent.SetHeight(math.max(0f, (float)SuggestionInstances.Count * (padding + height) - padding + headerHeight));
		base.gameObject.SetActiveSelfExt(SuggestionInstances.Count > 0);
	}
}
