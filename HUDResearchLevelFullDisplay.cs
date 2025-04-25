using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using TMPro;
using Unity.Core.View;
using UnityEngine;

public class HUDResearchLevelFullDisplay : HUDComponent
{
	[SerializeField]
	protected RectTransform UISideGoalsParent;

	[SerializeField]
	protected HUDShapeDisplay UILevelShapePreview;

	[SerializeField]
	protected TMP_Text UILevelTitle;

	[SerializeField]
	protected GameObject UINoSidegoalsIndicator;

	[SerializeField]
	protected PrefabViewReference<HUDResearchNodePreview> ResearchNodePreviewPrefab;

	protected List<HUDResearchNodePreview> UISideGoalInstances = new List<HUDResearchNodePreview>();

	private ShapeManager ShapeManager;

	private ResearchManager ResearchManager;

	private Player Player;

	private ResearchLevelHandle _level;

	public ResearchLevelHandle Level
	{
		get
		{
			return _level;
		}
		set
		{
			if (value != _level)
			{
				_level = value;
				UpdateView();
				UpdateSideGoals();
			}
		}
	}

	[Construct]
	private void Construct(ShapeManager shapeManager, ResearchManager researchManager, Player player)
	{
		ShapeManager = shapeManager;
		ResearchManager = researchManager;
		Player = player;
		UISideGoalsParent.RemoveAllChildren();
		AddChildView(UILevelShapePreview);
	}

	private void UpdateView()
	{
		UILevelShapePreview.Shape = ShapeManager.GetDefinitionByHash(Level.Cost.DefinitionHash);
		UILevelTitle.text = Level.Meta.Title;
	}

	protected override void OnDispose()
	{
		ClearSideGoals();
	}

	protected int ComputeSideGoalSortIndex(ResearchSideGoalHandle node)
	{
		int score = (ResearchManager.Progress.IsUnlocked(node) ? 1000 : 0);
		if (node.Meta.SpeedAdjustments.Count > 0)
		{
			score++;
		}
		return score;
	}

	protected void UpdateSideGoals()
	{
		ClearSideGoals();
		if (Level == null)
		{
			return;
		}
		IOrderedEnumerable<ResearchSideGoalHandle> sideGoals = Level.SideGoals.OrderBy(ComputeSideGoalSortIndex);
		foreach (ResearchSideGoalHandle node in sideGoals)
		{
			HUDResearchNodePreview sideGoalPreview = RequestChildView(ResearchNodePreviewPrefab).PlaceAt(UISideGoalsParent);
			sideGoalPreview.ShowPin = true;
			sideGoalPreview.ShowTooltip = true;
			sideGoalPreview.Research = node;
			sideGoalPreview.TranslucentBackground = false;
			sideGoalPreview.Clicked.AddListener(delegate
			{
				HandleSideGoalClick(node);
			});
			UISideGoalInstances.Add(sideGoalPreview);
		}
		UINoSidegoalsIndicator.SetActiveSelfExt(UISideGoalInstances.Count == 0);
	}

	protected void HandleSideGoalClick(ResearchSideGoalHandle research)
	{
		if (ResearchManager.CanUnlock(research))
		{
			if (ResearchManager.TryUnlock(research))
			{
				Globals.UISounds.PlayClick();
			}
			else
			{
				Globals.UISounds.PlayError();
			}
		}
		else if (ResearchManager.CanReach(research))
		{
			Player.Pins.TogglePinned(research);
			Globals.UISounds.PlayClick();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	protected void ClearSideGoals()
	{
		foreach (HUDResearchNodePreview goal in UISideGoalInstances)
		{
			ReleaseChildView(goal);
		}
		UISideGoalInstances.Clear();
	}
}
