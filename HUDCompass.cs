using System;
using Core.Dependency;
using DG.Tweening;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HUDCompass : HUDPart
{
	private static Plane GROUND_PLANE = new Plane(Vector3.up, Vector3.zero);

	[SerializeField]
	private RectTransform UICameraAngleParent;

	[SerializeField]
	private RectTransform UICameraRotationParent;

	[SerializeField]
	private RectTransform UIShapeParent;

	[SerializeField]
	private RectTransform UIHUBDirectionIndicator;

	[SerializeField]
	private CanvasGroup UIHUBDirectionIndicatorAlpha;

	[SerializeField]
	private Button UISnapSettingsBtn;

	[SerializeField]
	private TMP_Text UISnapSettingsText;

	[SerializeField]
	private Image UIZoomIndicator;

	[SerializeField]
	private RectTransform UISwitchIslandZoomIndicator;

	[SerializeField]
	private RectTransform UISwitchOverviewZoomIndicator;

	private ResearchManager Research;

	[Construct]
	private void Construct(ResearchManager research)
	{
		Research = research;
		Globals.Settings.Camera.ViewportSnapping.Changed.AddListener(OnSnappingChanged);
		Research.Progress.OnChanged.AddListener(UpdateShapePreviewMesh);
		UpdateShapePreviewMesh();
		PlayerViewport viewport = Player.Viewport;
		viewport.ZoomChanged.AddListener(OnZoomChanged);
		viewport.AngleChanged.AddListener(OnAngleOrRotationChanged);
		viewport.RotationChanged.AddListener(OnAngleOrRotationChanged);
		viewport.PositionChanged.AddListener(OnPositionChanged);
		UISnapSettingsBtn.onClick.AddListener(ToggleSnapping);
		PrepareZoomSwitchIndicators();
		OnZoomChanged();
		OnPositionChanged();
		OnAngleOrRotationChanged();
		OnSnappingChanged();
		UIHUBDirectionIndicatorAlpha.alpha = 0f;
	}

	protected override void OnDispose()
	{
		Globals.Settings.Camera.ViewportSnapping.Changed.RemoveListener(OnSnappingChanged);
		Research.Progress.OnChanged.RemoveListener(UpdateShapePreviewMesh);
		PlayerViewport viewport = Player.Viewport;
		viewport.ZoomChanged.RemoveListener(OnZoomChanged);
		viewport.AngleChanged.RemoveListener(OnAngleOrRotationChanged);
		viewport.RotationChanged.RemoveListener(OnAngleOrRotationChanged);
		viewport.PositionChanged.RemoveListener(OnPositionChanged);
		DOTween.Kill(UISnapSettingsBtn.transform, complete: true);
	}

	private void ToggleSnapping()
	{
		EnumGameSettingLegacy<int> setting = Globals.Settings.Camera.ViewportSnapping;
		int index = Array.IndexOf(setting.AvailableValues, setting.Current);
		int newIndex = FastMath.SafeMod(index - 1, setting.AvailableValues.Length);
		DynamicEnumGameSetting<int>.Entry newValue = setting.AvailableValues[newIndex];
		setting.TrySetFromString(newValue.ValueId);
		HUDTheme.AnimateElementInteracted(UISnapSettingsBtn.transform);
	}

	private void PrepareZoomSwitchIndicators()
	{
		UISwitchIslandZoomIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, (0f - ConvertZoomToPercentage(HUDScopeManager.BUILDINGS_MAX_ZOOM)) * 360f);
		UISwitchOverviewZoomIndicator.transform.localRotation = Quaternion.Euler(0f, 0f, (0f - ConvertZoomToPercentage(HUDScopeManager.OVERVIEW_ZOOM)) * 360f);
	}

	private float ConvertZoomToPercentage(float zoom)
	{
		return math.pow(math.saturate((zoom - HUDCameraManager.MIN_ZOOM) / (HUDCameraManager.MAX_ZOOM - HUDCameraManager.MIN_ZOOM)), 1f / 3f);
	}

	private void UpdateShapePreviewMesh()
	{
		UIShapeParent.transform.RemoveAllChildren();
		ResearchLevelHandle nextLevel = Singleton<GameCore>.G.Research.LevelManager.CurrentLevel;
		if (nextLevel != null && nextLevel.Cost.RequiresShape)
		{
			ShapeDefinition shapeDefinition = Singleton<GameCore>.G.Shapes.GetDefinitionByHash(nextLevel.Cost.DefinitionHash);
			Mesh shapeMesh = GeometryHelpers.GenerateTransformedMesh_UNCACHED(shapeDefinition.GetMesh(), Matrix4x4.identity);
			GameObject itemObj = new GameObject("CameraManagerShape")
			{
				layer = LayerMask.NameToLayer("UI")
			};
			itemObj.transform.SetParent(UIShapeParent, worldPositionStays: false);
			itemObj.transform.localPosition = Vector3.zero;
			itemObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
			itemObj.transform.localScale = new Vector3(260f, 260f, 260f);
			MeshFilter itemMeshObj = itemObj.AddComponent<MeshFilter>();
			itemMeshObj.sharedMesh = shapeMesh;
			MeshRenderer itemRenderObj = itemObj.AddComponent<MeshRenderer>();
			itemRenderObj.material = Globals.Resources.ShapeMaterialUI;
			itemRenderObj.receiveShadows = false;
			itemRenderObj.shadowCastingMode = ShadowCastingMode.On;
		}
	}

	private void UpdateHubIndicator()
	{
		float2 hubPosition_W = Player.CurrentMap.HUBEntity.W_From_L(new float3(0f, 0f, 0f)).xz;
		float2 cameraPos_W = FindCompassReferencePosition_W().xz;
		float2 vectorToHub_W = cameraPos_W - hubPosition_W;
		float alpha = math.saturate((math.distance(cameraPos_W, hubPosition_W) - 20f) / 30f);
		float angle = math.degrees(math.atan2(vectorToHub_W.y, vectorToHub_W.x)) + 90f;
		UIHUBDirectionIndicatorAlpha.alpha = alpha;
		UIHUBDirectionIndicator.localRotation = Quaternion.Euler(0f, 0f, angle);
	}

	private void OnPositionChanged()
	{
		UpdateHubIndicator();
	}

	private float3 FindCompassReferencePosition_W()
	{
		float2 pos = new float2((float)Screen.width * 0.05f, (float)Screen.height * 0.1f);
		Ray mouseRay = Player.Viewport.MainCamera.ScreenPointToRay(new float3(pos, 0f));
		if (!GROUND_PLANE.Raycast(mouseRay, out var enter))
		{
			Debug.LogWarning("compass no intersection");
			return new float3(0);
		}
		return mouseRay.GetPoint(enter);
	}

	private void OnAngleOrRotationChanged()
	{
		PlayerViewport viewport = Player.Viewport;
		UICameraRotationParent.localRotation = Quaternion.Euler(0f, 0f, viewport.RotationDegrees);
		UICameraAngleParent.localRotation = Quaternion.Euler(90f - viewport.Angle, 0f, 0f);
		UpdateHubIndicator();
	}

	private void OnZoomChanged()
	{
		PlayerViewport viewport = Player.Viewport;
		UIZoomIndicator.fillAmount = ConvertZoomToPercentage(viewport.Zoom);
		UpdateHubIndicator();
	}

	private void OnSnappingChanged()
	{
		string snappingText = StringFormatting.FormatIntegerRaw(Player.Viewport.SnappingDegrees);
		UISnapSettingsText.text = "camera.viewport-snap.degrees".tr(("<degrees>", snappingText));
	}
}
