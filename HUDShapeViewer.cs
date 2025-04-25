using System;
using System.Collections.Generic;
using Core.Dependency;
using Core.Events;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class HUDShapeViewer : HUDPart
{
	private struct LinkedShapePart
	{
		public int Layer;

		public int Index;

		public GameObject Handle;
	}

	[Serializable]
	public class ViewConfig
	{
		public float ShapeGap = 0f;

		public float LayerGap = 0f;

		public float SupportMeshDistance = 0f;

		public float OrientationY = 30f;

		public float AdditionalShapeDistanceToPlatform = 0f;

		public HUDIconButton LinkedButton;
	}

	[SerializeField]
	private HUDAnimatedRoundButton UICloseButton;

	[SerializeField]
	private CanvasGroup UIMainCanvasGroup;

	[SerializeField]
	private GameObject UIDialog;

	[SerializeField]
	private GameObject UIMainTitle;

	[SerializeField]
	private GameObject UIControlsParent;

	[SerializeField]
	private TMP_Text UIShortCodeText;

	[SerializeField]
	private HUDIconButton UICopyShortCodeButton;

	[SerializeField]
	private HUDIconButton UIEditShortCodeButton;

	[SerializeField]
	private HUDIconButton UICheatShapeButton;

	[SerializeField]
	private GameObject UIShapeParent;

	[SerializeField]
	private TMP_Text UIStoredAmountText;

	[SerializeField]
	public ViewConfig[] Views = new ViewConfig[0];

	private Sequence CurrentVisibilityAnimation;

	private Sequence CurrentShapeAnimation;

	private ShapeDefinition CurrentDefinition = null;

	private float ShapeScale = 1500f;

	private float OrientationVertical = 30f;

	private float OrientationHorizontal = 0f;

	private GameObject ShapeSupportMesh;

	private List<LinkedShapePart> LinkedShapeParts = new List<LinkedShapePart>();

	private ViewConfig FadedOutView = new ViewConfig
	{
		LayerGap = 2f,
		ShapeGap = 2f,
		SupportMeshDistance = 1f,
		AdditionalShapeDistanceToPlatform = 4f
	};

	private ViewConfig CurrentView = null;

	private IHUDDialogStack DialogStack;

	private IEventSender PassiveEventBus;

	private ResearchManager Research;

	[Construct]
	private void Construct(IHUDDialogStack dialogStack, IEventSender passiveEventBus, ResearchManager research)
	{
		AddChildView(UICloseButton);
		DialogStack = dialogStack;
		PassiveEventBus = passiveEventBus;
		Research = research;
		base.gameObject.SetActive(value: false);
		AddChildView(UICopyShortCodeButton);
		AddChildView(UIEditShortCodeButton);
		AddChildView(UICheatShapeButton);
		UICheatShapeButton.gameObject.SetActiveSelfExt(Application.isEditor);
		if (Application.isEditor)
		{
			UICheatShapeButton.Clicked.AddListener(CheatShapeAmount);
		}
		Events.ShowShapeViewer.AddListener(ShowFor);
		UICloseButton.Clicked.AddListener(Hide);
		UICopyShortCodeButton.Clicked.AddListener(CopyShapeCodeToClipboard);
		UIEditShortCodeButton.Clicked.AddListener(ShowShapeEditDialog);
		ViewConfig[] views = Views;
		foreach (ViewConfig view in views)
		{
			HUDIconButton btn = view.LinkedButton;
			AddChildView(btn);
			btn.Clicked.AddListener(delegate
			{
				TransitionToView(view, 0.5f, 0.1f);
			});
		}
	}

	private void CheatShapeAmount()
	{
		if (CurrentDefinition != null)
		{
			Research.ShapeStorage.Add(CurrentDefinition, 100000);
		}
	}

	protected override void OnDispose()
	{
		Events.ShowShapeViewer.RemoveListener(ShowFor);
		UICloseButton.Clicked.RemoveListener(Hide);
		UICopyShortCodeButton.Clicked.RemoveListener(CopyShapeCodeToClipboard);
		UIEditShortCodeButton.Clicked.RemoveListener(ShowShapeEditDialog);
		if (Application.isEditor)
		{
			UICheatShapeButton.Clicked.RemoveListener(CheatShapeAmount);
		}
	}

	public override void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
	{
		if (CurrentDefinition == null)
		{
			return;
		}
		if (context.ConsumeWasActivated("global.cancel"))
		{
			Hide();
			return;
		}
		int stored = Research.ShapeStorage.GetAmount(CurrentDefinition.Hash);
		UIStoredAmountText.text = StringFormatting.FormatShapeAmount(stored);
		float2 mouseDelta = context.ConsumeMouseDelta();
		if (math.abs(mouseDelta.y) > 0.01f && context.IsActive("shape-viewer.move-modifier"))
		{
			OrientationVertical = math.clamp(OrientationVertical + mouseDelta.y / (float)Screen.height * 250f, 0f, 80f);
			UpdateOrientation();
		}
		if (math.abs(mouseDelta.x) > 0.01f && context.IsActive("shape-viewer.move-modifier"))
		{
			OrientationHorizontal += mouseDelta.x / (float)Screen.width * 400f;
			UpdateOrientation();
		}
		context.ConsumeAll();
	}

	private void CopyShapeCodeToClipboard()
	{
		GUIUtility.systemCopyBuffer = CurrentDefinition.Hash;
	}

	private void ShowShapeEditDialog()
	{
		UIShapeParent.SetActiveSelfExt(active: false);
		HUDDialogSimpleInput dialog = DialogStack.ShowUIDialog<HUDDialogSimpleInput>();
		dialog.InitDialogContents("shape-viewer.edit-shape.title".tr(), "shape-viewer.edit-shape.description".tr(), "global.btn-ok".tr(), CurrentDefinition.Hash);
		dialog.CloseRequested.AddListener(delegate
		{
			UIShapeParent.SetActiveSelfExt(active: true);
		});
		dialog.OnConfirmed.AddListener(delegate(string text)
		{
			try
			{
				CurrentDefinition = new ShapeDefinition(text);
			}
			catch (Exception ex)
			{
				Debug.LogWarning("Not a valid shape code: " + ex);
				Globals.UISounds.PlayError();
			}
			RerenderDefinition();
		});
	}

	private void UpdateOrientation()
	{
		UIShapeParent.transform.localRotation = Quaternion.Euler(OrientationVertical, 0f, 0f) * Quaternion.Euler(0f, 0f, OrientationHorizontal);
	}

	private void UpdateUIViewButtons()
	{
		ViewConfig[] views = Views;
		foreach (ViewConfig view in views)
		{
			HUDIconButton btn = view.LinkedButton;
			if (view == CurrentView)
			{
			}
		}
	}

	private void ShowFor(ShapeDefinition definition)
	{
		if (CurrentDefinition != definition)
		{
			base.gameObject.SetActive(value: true);
			base.Raycaster.enabled = true;
			UIShapeParent.SetActiveSelfExt(active: true);
			CurrentDefinition = definition;
			CurrentVisibilityAnimation?.Kill();
			CurrentVisibilityAnimation = DOTween.Sequence();
			UIMainCanvasGroup.alpha = 0f;
			CurrentVisibilityAnimation.Append(UIMainCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutCubic));
			UIDialog.transform.localScale = new Vector3(1f, 0.2f, 1f);
			UIDialog.transform.localPosition = new Vector3(-40f, 0f, 0f);
			CurrentVisibilityAnimation.Join(UIDialog.transform.DOScaleY(1f, 0.2f).SetEase(Ease.OutBack));
			CurrentVisibilityAnimation.Join(UIDialog.transform.DOLocalMoveX(0f, 0.9f).SetEase(Ease.OutElastic));
			CurrentVisibilityAnimation.Join(HUDTheme.AnimateSideUITopIn(UIMainTitle));
			CurrentVisibilityAnimation.Join(HUDTheme.AnimateSideUIBottomIn(UIControlsParent));
			PassiveEventBus.Emit(new PlayerOpenedShapeViewerEvent(Player));
			RerenderDefinition();
		}
	}

	private void RerenderDefinition()
	{
		UIShortCodeText.text = CurrentDefinition.Hash;
		InitShapePartMeshes();
		TransitionToView(FadedOutView, 0.5f, 0.2f, instant: true);
		TransitionToView(Views[0], 0.4f);
	}

	private void Hide()
	{
		if (CurrentDefinition != null)
		{
			CurrentShapeAnimation?.Kill();
			CurrentVisibilityAnimation?.Kill();
			CurrentShapeAnimation = null;
			CurrentVisibilityAnimation = null;
			base.Raycaster.enabled = false;
			TransitionToView(FadedOutView, 0.4f, -0.1f);
			CurrentDefinition = null;
			CurrentVisibilityAnimation = DOTween.Sequence();
			CurrentVisibilityAnimation.Join(UIMainCanvasGroup.DOFade(0f, 0.4f).SetEase(Ease.OutCubic));
			CurrentVisibilityAnimation.Join(UIDialog.transform.DOScaleY(0f, 0.2f).SetEase(Ease.InBack));
			CurrentVisibilityAnimation.Join(UIDialog.transform.DOLocalMoveX(-500f, 0.2f).SetEase(Ease.InBack));
			CurrentVisibilityAnimation.Join(HUDTheme.AnimateSideUITopOut(UIMainTitle));
			CurrentVisibilityAnimation.Join(HUDTheme.AnimateSideUIBottomOut(UIControlsParent));
			CurrentVisibilityAnimation.OnComplete(delegate
			{
				base.gameObject.SetActive(value: false);
				UIShapeParent.transform.RemoveAllChildren();
				LinkedShapeParts.Clear();
				CurrentView = null;
			});
		}
	}

	private void InitShapePartMeshes()
	{
		UIShapeParent.transform.RemoveAllChildren();
		LinkedShapeParts.Clear();
		CurrentShapeAnimation?.Kill();
		CurrentShapeAnimation = null;
		GameResources config = Globals.Resources;
		ShapeSupportMesh = CreateMeshForShapeSubPart(ShapeItem.SUPPORT_MESH, new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f));
		ShapeDefinition shape = CurrentDefinition;
		ShapeLogic.UnfoldResult unfolded = ShapeLogic.Unfold(shape.Layers);
		foreach (ShapePartReference reference in unfolded.References)
		{
			ShapePart part = reference.Part;
			float baseModelHeight = 0.1f;
			float modelScale = config.ShapeDimensions2D / 0.37f * 0.5f;
			float rotation = (float)reference.PartIndex / (float)shape.PartCount * 360f;
			float scale = ShapeLogic.Logic_LayerScale(reference.LayerIndex) * modelScale;
			Mesh mesh = ShapeDefinition.CreateSubPartMesh_UNCACHED(part.Color, part.Shape);
			GameObject obj = CreateMeshForShapeSubPart(mesh, pos: new Vector3(0f, 0f, 0f), scale: new float3(scale, config.ShapeLayerHeight / baseModelHeight, scale), rotation: Quaternion.Euler(0f, rotation, 0f));
			LinkedShapeParts.Add(new LinkedShapePart
			{
				Layer = reference.LayerIndex,
				Index = reference.PartIndex,
				Handle = obj
			});
		}
	}

	private GameObject CreateMeshForShapeSubPart(Mesh mesh, Vector3 scale, Vector3 pos, Quaternion? rotation = null)
	{
		GameObject obj = new GameObject("ShapeMesh")
		{
			layer = LayerMask.NameToLayer("UI")
		};
		MeshFilter mf = obj.AddComponent<MeshFilter>();
		mf.sharedMesh = mesh;
		MeshRenderer mr = obj.AddComponent<MeshRenderer>();
		mr.receiveShadows = true;
		mr.shadowCastingMode = ShadowCastingMode.On;
		mr.sharedMaterial = Globals.Resources.ShapeMaterialUI;
		obj.transform.parent = UIShapeParent.transform;
		obj.transform.localPosition = pos * ShapeScale;
		obj.transform.localRotation = Quaternion.Euler(-90f, 90f, -90f) * (rotation ?? Quaternion.identity);
		obj.transform.localScale = scale * ShapeScale;
		return obj;
	}

	private void TransitionToView(ViewConfig view, float duration = 0.5f, float extraLayerDuration = 0.2f, bool instant = false)
	{
		if (view == CurrentView)
		{
			return;
		}
		CurrentView = view;
		CurrentShapeAnimation?.Kill();
		CurrentShapeAnimation = null;
		UpdateUIViewButtons();
		GameResources config = Globals.Resources;
		int partCount = CurrentDefinition.PartCount;
		float generalYOffset = (float)CurrentDefinition.Layers.Length / 8f * (config.ShapeLayerHeight + view.LayerGap);
		if (!instant)
		{
			CurrentShapeAnimation = DOTween.Sequence();
			CurrentShapeAnimation.Append(DOTween.To(() => OrientationVertical, delegate(float value)
			{
				OrientationVertical = value;
				UpdateOrientation();
			}, view.OrientationY, duration));
		}
		else
		{
			OrientationVertical = view.OrientationY;
			UpdateOrientation();
		}
		foreach (LinkedShapePart part in LinkedShapeParts)
		{
			float modelScale = config.ShapeDimensions2D / 0.37f * 0.5f;
			float rotationRad = math.radians(((float)part.Index + 0.5f) / (float)partCount * 360f);
			float gap = config.ShapeInnerGap * modelScale + view.ShapeGap * 0.1f;
			float3 targetPos = new float3(math.sin(rotationRad) * gap, math.cos(rotationRad) * gap, (float)(-part.Layer) * config.ShapeLayerHeight - (float)(part.Layer + 1) * view.LayerGap + generalYOffset - view.AdditionalShapeDistanceToPlatform) * ShapeScale;
			Transform targetTransform = part.Handle.transform;
			if (instant)
			{
				targetTransform.localPosition = targetPos;
			}
			else
			{
				CurrentShapeAnimation.Join(targetTransform.DOLocalMove(targetPos, duration + (float)part.Layer * extraLayerDuration).SetEase(Ease.InOutCubic));
			}
		}
		float supportMeshZ = (view.SupportMeshDistance + config.ShapeSupportHeight + generalYOffset) * ShapeScale;
		if (instant)
		{
			ShapeSupportMesh.transform.localPosition = new Vector3(0f, 0f, supportMeshZ);
		}
		else
		{
			CurrentShapeAnimation.Join(ShapeSupportMesh.transform.DOLocalMoveZ(supportMeshZ, duration).SetEase(Ease.InOutCubic));
		}
	}
}
