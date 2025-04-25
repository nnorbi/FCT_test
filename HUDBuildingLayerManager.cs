using System.Collections.Generic;
using Core.Dependency;
using DG.Tweening;
using Unity.Core.View;
using UnityEngine;
using UnityEngine.UI;

public class HUDBuildingLayerManager : HUDPart, IRunnableView, IView
{
	[SerializeField]
	private HUDIconButton UIBtnToggleLayerVisibility;

	[SerializeField]
	private Sprite UILayerVisibilityOnSprite;

	[SerializeField]
	private Sprite UILayerVisibilityOffSprite;

	[SerializeField]
	private HUDIconButton UIBtnLayerUp;

	[SerializeField]
	private HUDIconButton UIBtnLayerDown;

	[SerializeField]
	private Sprite UILayerSubSprite;

	[SerializeField]
	private Sprite UILayerTopSprite;

	[SerializeField]
	private RectTransform UILayerBtnParent;

	[SerializeField]
	private RectTransform UIMoverParent;

	protected bool Visible = true;

	protected List<Image> UILayerHandles = new List<Image>();

	private ResearchManager ResearchManager;

	private LayerManager LayerManager;

	public void Run()
	{
		InitializeLayersUI();
	}

	[Construct]
	private void Construct(ResearchManager researchManager, LayerManager layerManager)
	{
		AddChildView(UIBtnToggleLayerVisibility);
		AddChildView(UIBtnLayerUp);
		AddChildView(UIBtnLayerDown);
		ResearchManager = researchManager;
		LayerManager = layerManager;
		Player.Viewport.NewViewportLoaded.AddListener(OnViewportLoaded);
		Player.InputModeChanged.AddListener(OnInputModeChanged);
		UIBtnToggleLayerVisibility.Clicked.AddListener(UIToggleLayerVisibility);
		UIBtnLayerUp.Clicked.AddListener(GoLayerUp);
		UIBtnLayerDown.Clicked.AddListener(GoLayerDown);
		Player.Viewport.LayerChanged.AddListener(UpdateLayersState);
		Player.Viewport.ShowAllLayersChanged.AddListener(UpdateLayersState);
		ResearchManager.Progress.OnChanged.AddListener(UpdateLayersState);
		Player.MapChanged.AddListener(OnMapChanged);
	}

	protected override void OnDispose()
	{
		Player.Viewport.NewViewportLoaded.RemoveListener(OnViewportLoaded);
		Player.InputModeChanged.RemoveListener(OnInputModeChanged);
		UIBtnToggleLayerVisibility.Clicked.RemoveListener(UIToggleLayerVisibility);
		UIBtnLayerUp.Clicked.RemoveListener(GoLayerUp);
		UIBtnLayerDown.Clicked.RemoveListener(GoLayerDown);
		Player.Viewport.LayerChanged.RemoveListener(UpdateLayersState);
		Player.Viewport.ShowAllLayersChanged.RemoveListener(UpdateLayersState);
		ResearchManager.Progress.OnChanged.RemoveListener(UpdateLayersState);
		Player.MapChanged.RemoveListener(OnMapChanged);
	}

	private void OnViewportLoaded()
	{
		UpdateLayersState();
	}

	private void InitializeLayersUI()
	{
		for (short i = Singleton<GameCore>.G.Mode.MaxLayer; i >= 0; i--)
		{
			GameObject obj = new GameObject("layer-" + i, typeof(RectTransform), typeof(Image));
			RectTransform layerTransform = obj.GetComponent<RectTransform>();
			layerTransform.SetParent(UILayerBtnParent);
			layerTransform.localScale = new Vector3(1f, 1f, 1f);
			layerTransform.anchorMax = new Vector2(0.5f, 1f);
			layerTransform.anchorMin = new Vector2(0.5f, 1f);
			layerTransform.anchoredPosition = new Vector2(0f, -5 - ((i == 0) ? 12 : (24 + (i - 1) * 9)));
			layerTransform.sizeDelta = new Vector2(45f, 22f);
			Image image = obj.GetComponent<Image>();
			image.sprite = ((i == 0) ? UILayerTopSprite : UILayerSubSprite);
			image.material = Globals.Resources.DefaultUISpriteMaterial;
			image.preserveAspect = true;
			UILayerHandles.Add(image);
		}
		UpdateLayersState();
	}

	private void GoLayerUp()
	{
		if (Player.Viewport.Layer < Player.CurrentMap.InteractionMode.GetMaximumAllowedLayer(Player))
		{
			LayerManager.SwitchLayer((short)(Player.Viewport.Layer + 1));
			Globals.UISounds.PlayLayerSwitch();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	private void GoLayerDown()
	{
		if (Player.Viewport.Layer > 0)
		{
			LayerManager.SwitchLayer((short)(Player.Viewport.Layer - 1));
			Globals.UISounds.PlayLayerSwitch();
		}
		else
		{
			Globals.UISounds.PlayError();
		}
	}

	private void UISetVisible(bool visible)
	{
		if (Visible != visible)
		{
			Visible = visible;
			DOTween.Kill(UIMoverParent);
			Ease ease = (visible ? Ease.OutExpo : Ease.InQuad);
			float duration = (visible ? 0.15f : 0.2f);
			UIMoverParent.DOLocalMoveX((!visible) ? 200 : 0, duration).SetEase(ease);
			UIMoverParent.DOScaleY(visible ? 1f : 0f, duration).SetEase(ease);
		}
	}

	private void UIToggleLayerVisibility()
	{
		if (Player.CurrentMap.InteractionMode.GetMaximumAllowedLayer(Player) == 0)
		{
			Player.Viewport.ShowAllLayers = true;
			Globals.UISounds.PlayError();
		}
		else
		{
			Player.Viewport.ShowAllLayers = !Player.Viewport.ShowAllLayers;
			Globals.UISounds.PlayClick();
		}
	}

	private void OnMapChanged(GameMap arg0)
	{
		UpdateLayersState();
	}

	private void OnInputModeChanged(GameInputModeType mode)
	{
		UpdateLayersState();
	}

	private void UpdateLayersState()
	{
		short layer = Player.Viewport.Layer;
		short maxLayer = Player.CurrentMap.InteractionMode.GetMaximumAllowedLayer(Player);
		UIBtnToggleLayerVisibility.Active = !Player.Viewport.ShowAllLayers;
		UIBtnToggleLayerVisibility.HasBadge = !Player.Viewport.ShowAllLayers;
		UIBtnToggleLayerVisibility.Icon = (Player.Viewport.ShowAllLayers ? UILayerVisibilityOnSprite : UILayerVisibilityOffSprite);
		for (int i = 0; i < UILayerHandles.Count; i++)
		{
			Image handle = UILayerHandles[i];
			int offset = maxLayer - UILayerHandles.Count + 1;
			int effectiveLayer = i + offset;
			bool active = effectiveLayer >= 0 && effectiveLayer <= maxLayer;
			handle.gameObject.SetActive(active);
			DOTween.Kill(handle);
			if (active)
			{
				Color newColor = ((effectiveLayer == layer) ? HUDTheme.ColorIconButtonActive : ((!Player.Viewport.ShowAllLayers) ? ((effectiveLayer < layer) ? HUDTheme.ColorIconButtonIconOnly : HUDTheme.ColorIconButtonNormal.WithAlpha(0.1f)) : HUDTheme.ColorIconButtonIconOnly));
				handle.DOColor(newColor, 0.15f);
			}
		}
		UIBtnLayerUp.Interactable = layer < maxLayer;
		UIBtnLayerDown.Interactable = layer > 0;
		UILayerBtnParent.GetComponent<RectTransform>().SetHeight(32 + 10 * maxLayer);
		LayoutRebuilder.ForceRebuildLayoutImmediate(UILayerBtnParent);
		LayoutRebuilder.ForceRebuildLayoutImmediate(UILayerBtnParent);
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		short maxLayer = Player.CurrentMap.InteractionMode.GetMaximumAllowedLayer(Player);
		if (Player.Viewport.Scope != GameScope.Buildings)
		{
			maxLayer = 0;
		}
		if (maxLayer <= 0)
		{
			UISetVisible(visible: false);
			if (Player.Viewport.Layer != 0)
			{
				LayerManager.SwitchLayer(0);
			}
			if (!Player.Viewport.ShowAllLayers)
			{
				Player.Viewport.ShowAllLayers = true;
			}
			return;
		}
		UISetVisible(visible: true);
		if (context.ConsumeWasActivated("camera.select-layer-up"))
		{
			GoLayerUp();
		}
		if (context.ConsumeWasActivated("camera.select-layer-down"))
		{
			GoLayerDown();
		}
		if (context.ConsumeWasActivated("camera.toggle-show-all-layers"))
		{
			UIToggleLayerVisibility();
		}
	}
}
