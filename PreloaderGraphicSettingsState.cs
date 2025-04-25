using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using Unity.Core.View;
using UnityEngine;

public class PreloaderGraphicSettingsState : PreloaderState
{
	[SerializeField]
	private TMP_Text UILowEndGPUWarning;

	[SerializeField]
	private CanvasGroup UIPanelGroup;

	[SerializeField]
	private PrefabViewReference<PreloaderGraphicQualityPrefab> UIGraphicQualityPrefab;

	[SerializeField]
	private RectTransform UIPresetsParent;

	private Sequence Animation;

	private bool ActionTaken = false;

	private string GPUName => SystemInfo.graphicsDeviceName + " | " + SystemInfo.graphicsDeviceType.ToString() + " | " + SystemInfo.graphicsDeviceVendor;

	private void OnSelectedPreset(GraphicPreset preset)
	{
		if (!ActionTaken)
		{
			ActionTaken = true;
			Debug.Log("Selected graphics preset " + preset.Title);
			Globals.Settings.Graphics.ApplyGraphicPreset(preset);
			Animation?.Kill();
			Animation = DOTween.Sequence();
			AppendFadeoutToSequence(Animation, UIPanelGroup);
			Animation.OnComplete(PreloaderController.MoveToNextState);
		}
	}

	private bool CheckMinimumSpecs()
	{
		if (GameEnvironmentManager.FLAG_IGNORE_HARDWARE_CHECKS)
		{
			return true;
		}
		if (!SystemInfo.supportsComputeShaders)
		{
			PreloaderController.StopLoadingWithMessage("Your GPU ('" + GPUName + "') does not support compute shaders. Please ensure your system meets the minimum system requirements. You can try updating your Graphics Drivers.");
			return false;
		}
		if (SystemInfo.graphicsMemorySize < 1000)
		{
			PreloaderController.StopLoadingWithMessage("Your GPU ('" + GPUName + "') does not have enough VRAM (" + SystemInfo.graphicsMemorySize.ToString(CultureInfo.InvariantCulture) + " MB). 1024 MB VRAM is required at least. Please ensure your system meets the minimum system requirements.");
			return false;
		}
		if (SystemInfo.systemMemorySize < 4000)
		{
			PreloaderController.StopLoadingWithMessage("Your System does not have enough RAM (" + SystemInfo.systemMemorySize.ToString(CultureInfo.InvariantCulture) + " MB). 4096 MB RAM is required at least. Please ensure your system meets the minimum system requirements.");
			return false;
		}
		if (Application.platform == RuntimePlatform.WindowsPlayer && Environment.OSVersion.Version.Major < 10)
		{
			PreloaderController.StopLoadingWithMessage("Your operating system version is not supported. Please ensure your system meets the minimum system requirements (Windows 10 or newer).");
			return false;
		}
		return true;
	}

	public override void OnEnterState()
	{
		if (!CheckMinimumSpecs())
		{
			return;
		}
		if ((bool)Globals.Settings.General.PreloadIntroShown)
		{
			PreloaderController.MoveToNextState();
			return;
		}
		Cursor.visible = true;
		Animation = DOTween.Sequence();
		JoinFadeinToSequence(Animation, UIPanelGroup);
		if (IsLowEnd())
		{
			UILowEndGPUWarning.gameObject.SetActiveSelfExt(active: true);
			UILowEndGPUWarning.text = "preload.graphic-quality.low-end-gpu-warning".tr(("<gpu-name>", GPUName));
		}
		else
		{
			UILowEndGPUWarning.gameObject.SetActiveSelfExt(active: false);
		}
		foreach (GraphicPreset preset in GraphicSettings.PRESETS)
		{
			PreloaderGraphicQualityPrefab instance = RequestChildView(UIGraphicQualityPrefab).PlaceAt(UIPresetsParent);
			instance.Title = preset.Title;
			instance.Text = preset.Description;
			instance.Selected.AddListener(delegate
			{
				OnSelectedPreset(preset);
			});
		}
	}

	private bool IsLowEnd()
	{
		string rawGpu = GPUName.ToLower();
		return rawGpu.Contains("intel") || rawGpu.Contains("integrated") || rawGpu.Contains("laptop") || rawGpu.Contains("iris") || rawGpu.Contains("geforce mx") || rawGpu.Contains("0mx") || rawGpu.Contains("mobile");
	}

	protected override void OnDispose()
	{
		Animation?.Kill();
		Animation = null;
	}
}
