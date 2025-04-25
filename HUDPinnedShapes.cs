using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class HUDPinnedShapes : HUDComponent
{
	[SerializeField]
	protected PrefabViewReference<HUDResearchNodePreview> ResearchNodePreviewPrefab;

	[SerializeField]
	protected RectTransform UIItemsParent;

	protected List<HUDResearchNodePreview> PinInstances = new List<HUDResearchNodePreview>();

	private Player Player;

	private ResearchManager ResearchManager;

	private HUDEvents Events;

	[Construct]
	private void Construct(Player player, ResearchManager researchManager, HUDEvents hudEvents)
	{
		Player = player;
		ResearchManager = researchManager;
		Events = hudEvents;
		Player.Pins.OnResearchableAdded.AddListener(AddPinnedNode);
		Player.Pins.OnResearchableRemoved.AddListener(RemovePinnedNode);
		ResearchSideGoalHandle[] sideGoals = ResearchManager.Tree.SideGoals;
		foreach (ResearchSideGoalHandle node in sideGoals)
		{
			if (node.Cost.RequiresShape)
			{
				ResearchManager.ShapeStorage.AddFirstShapeStoredHook(node.Cost.DefinitionHash, delegate
				{
					PinNodeIfIsReachable(node);
				});
			}
		}
		foreach (IResearchableHandle node2 in Player.Pins.PinnedResearchables)
		{
			AddPinnedNode(node2);
		}
		UpdateLayout();
	}

	protected override void OnDispose()
	{
		Player.Pins.OnResearchableAdded.RemoveListener(AddPinnedNode);
		Player.Pins.OnResearchableRemoved.RemoveListener(RemovePinnedNode);
		PinInstances.Clear();
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		UnpinCompletedNodes();
		PinUnlockableResearch();
	}

	protected void PinNodeIfIsReachable(IResearchableHandle node)
	{
		if (ResearchManager.CanReach(node) && !ResearchManager.Progress.IsUnlocked(node))
		{
			Player.Pins.TryPin(node);
		}
	}

	protected void UpdateLayout()
	{
		float padding = UIItemsParent.GetComponent<VerticalLayoutGroup>().spacing;
		float height = ResearchNodePreviewPrefab.Resolve().GetComponent<RectTransform>().sizeDelta.y;
		float headerHeight = 20f + padding;
		UIItemsParent.SetHeight(math.max(0f, (float)PinInstances.Count * (padding + height) - padding + headerHeight));
		base.gameObject.SetActiveSelfExt(PinInstances.Count > 0);
	}

	protected void AddPinnedNode(IResearchableHandle researchable)
	{
		if (PinInstances.Any((HUDResearchNodePreview pin) => pin.Research == researchable))
		{
			base.Logger.Warning?.Log("Can not pin item twice: " + researchable);
			return;
		}
		HUDResearchNodePreview instance = RequestChildView(ResearchNodePreviewPrefab).PlaceAt(UIItemsParent);
		instance.Research = researchable;
		instance.ShowPin = false;
		instance.ShowTooltip = true;
		instance.Clicked.AddListener(delegate
		{
			OnPinnedNodeClicked(researchable);
		});
		PinInstances.Add(instance);
		UpdateLayout();
	}

	protected void OnPinnedNodeClicked(IResearchableHandle node)
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
		else
		{
			Events.ShowResearchAndHighlight.Invoke(node);
		}
	}

	protected void RemovePinnedNode(IResearchableHandle node)
	{
		int index = PinInstances.FindIndex((HUDResearchNodePreview hUDResearchNodePreview) => hUDResearchNodePreview.Research == node);
		if (index < 0)
		{
			base.Logger.Warning?.Log("Can not unpin item, is not pinned: " + node);
			return;
		}
		HUDResearchNodePreview instance = PinInstances[index];
		PinInstances.RemoveAt(index);
		ReleaseChildView(instance);
		UpdateLayout();
	}

	protected void UnpinCompletedNodes()
	{
		List<IResearchableHandle> pinned = Player.Pins.PinnedResearchables.ToList();
		foreach (IResearchableHandle goal in pinned)
		{
			if (ResearchManager.Progress.IsUnlocked(goal) && !Player.Pins.TryUnpin(goal))
			{
				Debug.LogWarning("Failed to unpin completed node: " + goal.Meta.Title);
			}
		}
	}

	protected void PinUnlockableResearch()
	{
		ResearchManager research = ResearchManager;
		ResearchSideGoalHandle[] sideGoals = research.Tree.SideGoals;
		foreach (ResearchSideGoalHandle node in sideGoals)
		{
			if (research.CanUnlock(node) && !Player.Pins.IsPinned(node))
			{
				Player.Pins.TryPin(node);
			}
		}
	}
}
