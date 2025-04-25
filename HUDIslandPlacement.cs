using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class HUDIslandPlacement : HUDPartWithSidePanel
{
	[SerializeField]
	private RectTransform UIChunkLimitParent;

	[SerializeField]
	private CanvasGroup UIChunkLimitCanvasGroup;

	[SerializeField]
	private TMP_Text UIChunkLimitText;

	[SerializeField]
	private HUDIconButton UIChunkLimitButton;

	[SerializeField]
	private HUDCostDisplayComponent UIChunkCostDisplay;

	protected IslandPlacementBehaviour Behaviour;

	protected bool ChunkLimitVisible = true;

	protected Sequence ChunkLimitAnimation = null;

	protected IslandPlacementBehaviour.PersistentPlacementData LastPlacementData = new IslandPlacementBehaviour.PersistentPlacementData
	{
		Rotation = Grid.Direction.Right
	};

	[Construct]
	private void Construct()
	{
		AddChildView(UIChunkLimitButton);
		AddChildView(UIChunkCostDisplay);
		UIChunkCostDisplay.Hide();
		Player.SelectedIslandLayout.Changed.AddListener(OnSelectedIslandLayoutChanged);
		UpdateChunkLimit();
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		Player.SelectedIslandLayout.Changed.RemoveListener(OnSelectedIslandLayoutChanged);
	}

	protected void OnSelectedIslandLayoutChanged(MetaIslandLayout layout)
	{
		InternalClearPlacementBehaviour();
		if (layout == null)
		{
			UIChunkCostDisplay.Hide();
			return;
		}
		IslandPlacementBehaviour.CtorData ctorData = new IslandPlacementBehaviour.CtorData
		{
			Player = Player,
			Layout = layout,
			PersistentData = LastPlacementData
		};
		Behaviour = ((Player.SelectedIslandLayout.Value is MetaHyperBeltLayout) ? ((IslandPlacementBehaviour)new HyperBeltPlacementBehavior(ctorData)) : ((IslandPlacementBehaviour)new RegularIslandPlacementBehaviour(ctorData)));
		SidePanel_MarkDirty();
	}

	protected void SetChunkLimitVisible(bool visible)
	{
		if (ChunkLimitVisible == visible)
		{
			return;
		}
		ChunkLimitVisible = visible;
		if (ChunkLimitVisible)
		{
			base.gameObject.SetActive(value: true);
			ChunkLimitAnimation?.Kill();
			ChunkLimitAnimation = DOTween.Sequence();
			ChunkLimitAnimation.Append(UIChunkLimitParent.DOLocalMoveY(0f, 0.3f).SetEase(Ease.OutExpo));
			ChunkLimitAnimation.Join(UIChunkLimitCanvasGroup.DOFade(1f, 0.3f));
			return;
		}
		float duration = 0.2f;
		ChunkLimitAnimation = DOTween.Sequence();
		ChunkLimitAnimation.Append(UIChunkLimitParent.DOLocalMoveY(150f, duration).SetEase(Ease.InSine));
		ChunkLimitAnimation.Join(UIChunkLimitCanvasGroup.DOFade(0f, duration));
		ChunkLimitAnimation.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	protected void UpdateChunkLimit()
	{
		ResearchChunkLimitManager chunks = Singleton<GameCore>.G.Research.ChunkLimitManager;
		string limit = StringFormatting.FormatGenericCount(chunks.CurrentChunkLimit);
		string current = StringFormatting.FormatGenericCount(chunks.ComputeCurrentChunkUsage());
		UIChunkLimitText.text = "<b>" + current + "</b> / " + limit;
	}

	protected void InternalClearPlacementBehaviour()
	{
		if (Behaviour != null)
		{
			LastPlacementData = Behaviour.GetPersistentData();
			Behaviour.Cleanup();
			Behaviour = null;
			SidePanel_MarkDirty();
		}
	}

	protected void StopPlacement()
	{
		Player.SelectedIslandLayout.Value = null;
		UIChunkCostDisplay.Hide();
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		base.OnGameUpdate(context, drawOptions);
		if (Player.Viewport.Scope != GameScope.Islands || !Player.CurrentMap.InteractionMode.AllowIslandManagement(Player))
		{
			SetChunkLimitVisible(visible: false);
			return;
		}
		SetChunkLimitVisible(Singleton<GameCore>.G.Research.Progress.IsUnlocked(Singleton<GameCore>.G.Mode.ResearchConfig.IslandManagementUnlock));
		if (ChunkLimitVisible)
		{
			UpdateChunkLimit();
		}
		if (context.IsTokenAvailable("HUDPart$main_interaction") && context.ConsumeWasActivated("mass-selection.pipette"))
		{
			HandlePipette();
		}
		if (Behaviour != null)
		{
			if (context.ConsumeAllCheckOneActivated("global.cancel", "building-placement.cancel-placement"))
			{
				StopPlacement();
				return;
			}
			if (!context.ConsumeToken("HUDPart$main_interaction"))
			{
				StopPlacement();
				return;
			}
			if (Behaviour.Update(context, drawOptions) == IslandPlacementBehaviour.UpdateResult.Stop)
			{
				StopPlacement();
				return;
			}
			int chunkCost = Behaviour.GetChunkCost();
			UIChunkCostDisplay.ShowAndUpdate(StringFormatting.FormatGenericCount(chunkCost), !Singleton<GameCore>.G.Research.ChunkLimitManager.CanAfford(chunkCost));
		}
	}

	private void HandlePipette()
	{
		if (!TryFindPipetteTarget(out var pipetteTarget))
		{
			StopPlacement();
		}
		else if (!Singleton<GameCore>.G.Research.Progress.IsUnlocked(pipetteTarget.Item1))
		{
			Globals.UISounds.PlayError();
			StopPlacement();
		}
		else
		{
			LastPlacementData.Rotation = pipetteTarget.Item2;
			Player.SelectedIslandLayout.Value = pipetteTarget.Item1;
		}
	}

	private bool TryFindPipetteTarget(out (MetaIslandLayout, Grid.Direction) target)
	{
		if (!ScreenUtils.TryGetTileAtCursor(Player, Player.Viewport.Layer, out var globalTile))
		{
			target = default((MetaIslandLayout, Grid.Direction));
			return false;
		}
		Island island = globalTile.Island;
		if (island == null)
		{
			target = default((MetaIslandLayout, Grid.Direction));
			return false;
		}
		MetaIslandLayout layout = island.Metadata.Layout;
		if (!layout.PlayerBuildable)
		{
			target = default((MetaIslandLayout, Grid.Direction));
			return false;
		}
		target = (layout, island.Metadata.LayoutRotation);
		return true;
	}

	protected override bool SidePanel_ShouldShow()
	{
		return Behaviour != null && Player.Viewport.Scope == GameScope.Islands;
	}

	protected override string SidePanel_GetTitle()
	{
		return Player.SelectedIslandLayout.Value?.Title;
	}

	protected override IEnumerable<HUDSidePanelHotkeyInfoData> SidePanel_GetActions()
	{
		return Behaviour.GetActions();
	}

	protected override IEnumerable<HUDSidePanelModule> SidePanel_GetModules()
	{
		MetaIslandLayout layout = Player.SelectedIslandLayout.Value;
		List<HUDSidePanelModule> modules = new List<HUDSidePanelModule>();
		modules.Add(new HUDSidePanelModuleInfoText(layout.Description));
		if (!(layout is MetaHyperBeltLayout))
		{
			modules.Add(new HUDSidePanelModuleStats(new HUDSidePanelModuleBaseStat[1]
			{
				new HUDSidePanelModuleStatChunkCount(layout.ChunkCount)
			}));
		}
		return modules;
	}
}
