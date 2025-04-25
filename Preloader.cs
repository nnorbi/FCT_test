using System;
using Core.Dependency;
using Core.Logging;
using DG.Tweening;
using TMPro;
using Unity.Core.Logging;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour, IPreloaderController
{
	[SerializeField]
	private TMP_Text UIVersionText;

	[Header("UI")]
	[Space(20f)]
	[SerializeField]
	private CanvasGroup UIInitialGroup;

	[SerializeField]
	private RectTransform UIStateParent;

	[SerializeField]
	private PreloaderState[] StatePrefabs;

	[SerializeField]
	private HUDCrashOverlay UICrashPrefab;

	[SerializeField]
	private PreloaderMessageDialog UIMessageDialogPrefab;

	[Header("Rendering")]
	[Space(20f)]
	[SerializeField]
	private Camera RendererCamera;

	[SerializeField]
	private UniversalRenderPipelineAsset RendererAsset;

	private DependencyContainer DependencyContainer;

	private Core.Logging.ILogger Logger;

	private int StateIndex = -1;

	private bool Crashed = false;

	private PreloaderState CurrentState;

	public void Start()
	{
		Debug.Log("Preload::Start");
		Logger = new UnityLogger();
		DependencyContainer = new DependencyContainer();
		DependencyContainer.Bind<Core.Logging.ILogger>().To(Logger);
		DependencyContainer.Bind<IPreloaderController>().To(this);
		Cursor.visible = false;
		Debug.Log("Preload::Fetch build info");
		GameEnvironmentManager.ReadBuildDate();
		UIVersionText.text = GameEnvironmentManager.DETAILED_VERSION;
		if (GameEnvironmentManager.FLAG_CLEAN_START)
		{
			Debug.Log("Preload::Clear all player prefs due to command line flag");
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
		}
		Debug.Log("Preload::Load mods");
		ModsLoader.Load();
		Debug.Log("Preload::Init settings");
		Globals.InitSettings();
		Debug.Log("Preload::Init display");
		InitDisplayAndGraphics();
		Debug.Log("Preload::Start");
		UIInitialGroup.alpha = 0f;
		UIInitialGroup.DOFade(1f, 2.5f);
		MoveToNextState();
	}

	public void Update()
	{
		if (!Crashed && Input.GetKeyDown(KeyCode.Escape))
		{
			CurrentState?.OnFastForwardRequested();
		}
	}

	public void MoveToNextState()
	{
		if (Crashed)
		{
			return;
		}
		if (CurrentState != null)
		{
			try
			{
				UnmountState(CurrentState);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to unload state: " + ex.Message);
			}
			CurrentState = null;
		}
		StateIndex++;
		if (StateIndex >= StatePrefabs.Length)
		{
			MoveToMainMenu();
			return;
		}
		PreloaderState prefab = StatePrefabs[StateIndex];
		CurrentState = UnityEngine.Object.Instantiate(prefab, UIStateParent);
		try
		{
			MountState(CurrentState);
		}
		catch (Exception ex2)
		{
			CrashWithMessage("Failed to mount " + ex2);
		}
	}

	public void CrashWithMessage(string message)
	{
		if (!Crashed)
		{
			Crashed = true;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Debug.LogError("PRELOAD CRASH: " + message);
			HUDCrashOverlay instance = UnityEngine.Object.Instantiate(UICrashPrefab, base.transform);
			instance.Setup(message);
			CurrentState?.gameObject.SetActiveSelfExt(active: false);
		}
	}

	public void StopLoadingWithMessage(string message)
	{
		if (!Crashed)
		{
			Crashed = true;
			Cursor.visible = true;
			Cursor.lockState = CursorLockMode.None;
			Debug.LogWarning("PRELOAD STOP: " + message);
			PreloaderMessageDialog instance = UnityEngine.Object.Instantiate(UIMessageDialogPrefab, base.transform);
			instance.Setup(message);
			CurrentState?.gameObject.SetActiveSelfExt(active: false);
		}
	}

	private void InitDisplayAndGraphics()
	{
		DisplaySettings displaySettings = Globals.Settings.Display;
		GraphicSettings graphicSettings = Globals.Settings.Graphics;
		GraphicSettings settings = new GraphicSettings(saveOnChange: false);
		settings.ApplyGraphicPreset(GraphicSettings.PRESETS[0]);
		settings.Apply(RendererCamera, RendererAsset);
		if (GameEnvironmentManager.FLAG_SAFE_MODE)
		{
			Debug.Log("Preload:: Enabling safe mode (windowed mode at 1024x768 and minimal graphics)");
			displaySettings.Resolution.SetValue(new DisplayResolution(1280, 720));
			displaySettings.WindowMode.SetValue(DisplayFullScreenMode.Windowed);
			graphicSettings.ApplyGraphicPreset(GraphicSettings.PRESETS[0]);
		}
		Logger.Info?.Log("Init display settings");
		displaySettings.InitDisplay();
	}

	private void MountState(PreloaderState state)
	{
		Debug.Log("Mount " + state.name);
		state.gameObject.SetActiveSelfExt(active: true);
		DependencyContainer.Inject(state);
		state.OnEnterState();
	}

	private void UnmountState(PreloaderState state)
	{
		Debug.Log("Unmount " + state.name);
		state.OnLeaveState();
		state.gameObject.SetActiveSelfExt(active: false);
		state.Dispose();
		UnityEngine.Object.Destroy(state);
	}

	private void MoveToMainMenu()
	{
		if (!Crashed)
		{
			UIInitialGroup.DOFade(0f, 0.5f).OnComplete(DoMoveToMainMenu);
		}
	}

	private void DoMoveToMainMenu()
	{
		if (!Crashed)
		{
			Globals.Settings.General.PreloadIntroShown.SetValue(value: true);
			DOTween.KillAll();
			SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
		}
	}
}
