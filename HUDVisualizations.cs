using System;
using System.Collections.Generic;
using Core.Dependency;
using Core.Factory;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;

public class HUDVisualizations : HUDPart
{
	protected class VisualizationInstance
	{
		public HUDIconButton Button;

		public HUDVisualization Visualization;

		[Construct]
		private object Construct()
		{
			return Visualization;
		}
	}

	public RectTransform UIVisualizationsParent;

	public PrefabViewReference<HUDIconButton> UIIconButtonPrefab;

	public CanvasGroup UIMainCanvasGroup;

	protected bool Visible = false;

	protected Sequence CurrentAnimation = null;

	protected List<VisualizationInstance> Visualizations = new List<VisualizationInstance>();

	private IFactory Factory;

	[Construct]
	private IEnumerable<VisualizationInstance> Construct(IFactory factory)
	{
		Factory = factory;
		AddVisualization<HUDIslandGridVisualization>(defaultActive: true);
		AddVisualization<HUDShapeResourcesVisualization>();
		AddVisualization<HUDIslandShapeTransferVisualization>();
		AddVisualization<HUDTunnelsVisualization>();
		AddVisualization<HUDTrainItemsVisualization>();
		AddVisualization<HUDTrainLinesVisualization>();
		AddVisualization<HUDTrainStationsVisualization>();
		UIMainCanvasGroup.alpha = 0f;
		UIVisualizationsParent.SetLocalPositionXOnly(150f);
		return Visualizations;
	}

	protected void AddVisualization<TVisualization>(bool defaultActive = false) where TVisualization : HUDVisualization
	{
		TVisualization visualization = Factory.Create<TVisualization>();
		HUDIconButton button = RequestChildView(UIIconButtonPrefab).PlaceAt(UIVisualizationsParent);
		VisualizationInstance instance = new VisualizationInstance
		{
			Visualization = visualization,
			Button = button
		};
		button.Icon = Globals.Resources.UIGlobalIconMapping.Get(visualization.GetGlobalIconId());
		button.TooltipTitle = visualization.GetTitle();
		button.TooltipTranslateTexts = false;
		button.TooltipAlignment = HUDTooltip.TooltipAlignment.Right_Middle;
		button.HasTooltip = true;
		button.Clicked.AddListener(instance.Visualization.ToggleEnabled);
		instance.Visualization.LinkWithHUD(Events, Player);
		if (defaultActive)
		{
			instance.Visualization.SetEnabled(enabled: true);
		}
		Visualizations.Add(instance);
	}

	protected void SetVisible(bool visible)
	{
		if (Visible == visible)
		{
			return;
		}
		Visible = visible;
		CurrentAnimation?.Kill();
		CurrentAnimation = DOTween.Sequence();
		if (Visible)
		{
			base.gameObject.SetActive(value: true);
			CurrentAnimation.Append(UIVisualizationsParent.DOLocalMoveX(0f, 0.3f).SetEase(Ease.OutExpo));
			CurrentAnimation.Join(UIMainCanvasGroup.DOFade(1f, 0.3f));
			return;
		}
		CurrentAnimation.Append(UIVisualizationsParent.DOLocalMoveX(150f, 0.3f).SetEase(Ease.OutExpo));
		CurrentAnimation.Join(UIMainCanvasGroup.DOFade(0f, 0.3f));
		CurrentAnimation.OnComplete(delegate
		{
			base.gameObject.SetActive(value: false);
		});
	}

	protected override void OnDispose()
	{
		foreach (VisualizationInstance visualization in Visualizations)
		{
			if (visualization is IDisposable disposableVisualization)
			{
				disposableVisualization.Dispose();
			}
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		bool anyToggleable = false;
		foreach (VisualizationInstance instance in Visualizations)
		{
			HUDVisualization visualization = instance.Visualization;
			bool available = visualization.IsAvailable();
			bool forced = visualization.IsForcedActive();
			bool userEnabled = visualization.UserEnabled;
			if (forced)
			{
				visualization.SetActive(active: true);
				instance.Button.gameObject.SetActiveSelfExt(active: false);
			}
			else if (!available)
			{
				visualization.SetActive(active: false);
				instance.Button.gameObject.SetActiveSelfExt(active: false);
			}
			else
			{
				visualization.SetActive(userEnabled);
				anyToggleable = true;
				instance.Button.gameObject.SetActiveSelfExt(active: true);
				instance.Button.Active = userEnabled;
			}
			instance.Visualization.OnGameUpdate(context, drawOptions);
		}
		SetVisible(anyToggleable);
	}
}
