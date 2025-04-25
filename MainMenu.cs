using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Dependency;
using Core.Logging;
using DG.Tweening;
using Unity.Core.Logging;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[DefaultExecutionOrder(-25)]
public class MainMenu : MonoBehaviour, IMainMenuStateControl
{
	[Serializable]
	public struct StatePosition
	{
		public Vector3 CameraPosition;

		public Vector3 CameraRotation;
	}

	private static readonly float STATE_TRANSITIONS_TIME = 1.1f;

	[FormerlySerializedAs("MenuMode")]
	[Header("Menu Savegame Config")]
	[Space(20f)]
	[SerializeField]
	private MenuBackgroundSavegame BackgroundSavegame;

	[Header("UI")]
	[Space(20f)]
	[SerializeField]
	private GameObject UIMainMover;

	[SerializeField]
	private CanvasGroup UIFadeInPanel;

	[SerializeField]
	private CanvasGroup UIFadeOutPanel;

	[SerializeField]
	private Transform UIDialogsParent;

	[Header("States")]
	[Space(20f)]
	[SerializeField]
	private MainMenuState[] UIStates;

	[SerializeField]
	private EditorDict<string, StatePosition> UIStatePositions = new EditorDict<string, StatePosition>();

	[Header("Rendering")]
	[Space(20f)]
	[SerializeField]
	private GameObject LoadingCamera;

	private HUDDialogStack DialogStack;

	private DependencyContainer DependencyContainer;

	private UnityLogger Logger;

	private MainMenuState CurrentState;

	private Sequence CurrentCameraAnimation;

	private Sequence CurrentStateAnimation;

	private GameInputManager InputManager;

	private float LastTransition = -1E+10f;

	private List<HUDDialog> ActiveDialogs = new List<HUDDialog>();

	private UnityEvent<GameObject, Action<HUDDialog>> ShowDialogEvent = new UnityEvent<GameObject, Action<HUDDialog>>();

	private void Start()
	{
		Logger = new UnityLogger();
		DialogStack = new HUDDialogStack(ShowDialogEvent);
		DependencyContainer = new DependencyContainer();
		DependencyContainer.Bind<Core.Logging.ILogger>().To(Logger);
		DependencyContainer.Bind<IMainMenuStateControl>().To(this);
		DependencyContainer.Bind<IHUDDialogStack>().To(DialogStack);
		DependencyContainer.Bind<ITutorialHighlightProvider>().To(new NoTutorialProvider());
		UIFadeInPanel.alpha = 1f;
		UIFadeOutPanel.alpha = 0f;
		Cursor.visible = false;
		ShowDialogEvent.AddListener(ShowDialog);
		StartCoroutine("Step0_InitGlobals");
	}

	private void Update()
	{
		if (Singleton<GameCore>.HasInstance && InputManager != null)
		{
			InputManager.OnGameUpdate();
			InputDownstreamContext context = InputManager.DownstreamContext;
			if (ActiveDialogs.Count > 0)
			{
				List<HUDDialog> activeDialogs = ActiveDialogs;
				activeDialogs[activeDialogs.Count - 1].OnGameUpdate(context);
			}
			if (context.ConsumeWasActivated("global.cancel"))
			{
				CurrentState.GoBack();
			}
			else if (CurrentStateAnimation == null)
			{
				CurrentState.DoUpdate(context);
			}
		}
	}

	private void ShowDialog(GameObject dialog, Action<HUDDialog> callback)
	{
		GameObject instance = UnityEngine.Object.Instantiate(dialog, UIDialogsParent);
		HUDDialog uiDialog = instance.GetComponent<HUDDialog>();
		uiDialog.CloseRequested.AddListener(delegate
		{
			uiDialog.Hide(destroyOnComplete: true);
			if (ActiveDialogs.Contains(uiDialog))
			{
				ActiveDialogs.Remove(uiDialog);
			}
		});
		uiDialog.Show();
		ActiveDialogs.Add(uiDialog);
		callback(uiDialog);
	}

	private void PrepareCameraForMenu()
	{
		Debug.Log("MenuController::Preparing camera for menu");
		UnityEngine.Object.Destroy(LoadingCamera);
		Player player = Singleton<GameCore>.G.LocalPlayer;
		GameRenderingSetup.SetupCameraStack(player, out var _);
	}

	private IEnumerator Step0_InitGlobals()
	{
		Logger.Debug?.Log("MenuController::Step 0: Init Globals");
		foreach (string item in Globals.Init())
		{
			yield return item;
		}
		Logger.Debug?.Log("MenuController::Step 1: Start");
		Globals.Settings.Display.InitDisplay();
		CurrentState = UIStates[0];
		UIMainMover.transform.localPosition = -CurrentState.GetVirtualUIWorldPosition();
		MainMenuState[] uIStates = UIStates;
		foreach (MainMenuState state in uIStates)
		{
			state.SetVisibleAndEnabled(active: false);
			state.gameObject.SetActive(value: false);
			state.gameObject.SetActive(value: true);
			state.gameObject.SetActive(value: false);
		}
		UIStates[0].SetVisibleAndEnabled(active: true);
		IEnumerator loader = BackgroundSavegame.Load();
		while (loader.MoveNext())
		{
			yield return loader.Current;
		}
		Debug.Log("MenuController::Step 2: Game loaded");
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
		PrepareCameraForMenu();
		InputManager = new GameInputManager(Singleton<GameCore>.G.LocalPlayer, Globals.Keybindings);
		DependencyContainer.Bind<ISavegameNameProvider>().To(Globals.Savegames);
		DependencyContainer.Bind<ISavegameManager>().To(Globals.Savegames);
		MainMenuState[] uIStates2 = UIStates;
		foreach (MainMenuState state2 in uIStates2)
		{
			DependencyContainer.Inject(state2);
		}
		Camera cam = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
		StatePosition statePos = UIStatePositions.Get(UIStates[0].name);
		cam.transform.position = statePos.CameraPosition;
		cam.transform.rotation = Quaternion.Euler(statePos.CameraRotation);
		UIFadeInPanel.DOFade(0f, 1f).SetEase(Ease.InOutCubic).OnComplete(delegate
		{
			SwitchToState<MenuMenuState>();
		});
	}

	private MainMenuState GetStateByType<T>() where T : MainMenuState
	{
		Type t = typeof(T);
		return UIStates.First((MainMenuState s) => t.IsInstanceOfType(s));
	}

	public Sequence SwitchToState<T>(object payload = null) where T : MainMenuState
	{
		MainMenuState newState = GetStateByType<T>();
		if (newState == CurrentState)
		{
			return null;
		}
		if (Time.time - LastTransition < 0.5f && (bool)Globals.Settings.Interface.MenuTransitions)
		{
			Debug.LogWarning("Preventing double transition");
			return null;
		}
		LastTransition = Time.time;
		CurrentStateAnimation?.Kill(complete: true);
		CurrentCameraAnimation?.Kill();
		MainMenuState oldState = CurrentState;
		CurrentState = newState;
		oldState.OnMenuLeaveState();
		newState.OnMenuEnterState(payload);
		newState.SetVisibleAndEnabled(active: true);
		StatePosition newStatePos = UIStatePositions.Get(newState.name);
		Ease ease = Ease.InOutCubic;
		Camera cam = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
		CurrentCameraAnimation = DOTween.Sequence();
		float transitionTime = STATE_TRANSITIONS_TIME;
		if (!Globals.Settings.Interface.MenuTransitions)
		{
			transitionTime = 0.05f;
		}
		else
		{
			Globals.UISounds.PlayMenuStateTransition();
		}
		CurrentCameraAnimation.Append(UIMainMover.transform.DOLocalMove(-newState.GetVirtualUIWorldPosition(), transitionTime).SetEase(ease));
		CurrentCameraAnimation.Join(cam.transform.DOMove(newStatePos.CameraPosition, transitionTime).SetEase(ease));
		CurrentCameraAnimation.Join(cam.transform.DORotateQuaternion(Quaternion.Euler(newStatePos.CameraRotation), transitionTime).SetEase(ease));
		CurrentStateAnimation = DOTween.Sequence();
		CurrentStateAnimation.AppendInterval(transitionTime);
		CurrentStateAnimation.OnComplete(delegate
		{
			newState.OnMenuEnterStateCompleted();
			oldState.SetVisibleAndEnabled(active: false);
			CurrentStateAnimation = null;
		});
		return CurrentCameraAnimation;
	}

	private Sequence FadeOut()
	{
		return SwitchToState<FadeOutMenuState>()?.AppendCallback(delegate
		{
			Singleton<GameCore>.G.Music.OnPrepareLeaveGame();
		}).Append(UIFadeOutPanel.DOFade(1f, MusicManager.LEAVE_GAME_FADEOUT_DURATION).SetEase(Ease.InOutCubic));
	}

	public void StartNewGame(GameModeConfig config)
	{
		FadeOut()?.OnComplete(delegate
		{
			Globals.CurrentGameStartOptionsPassOver = new GameStartOptionsStartNew
			{
				Config = config,
				MenuMode = false,
				UID = Globals.Savegames.GenerateNewUID()
			};
			DOTween.KillAll();
			Singleton<GameCore>.G.Dispose();
			SceneManager.LoadScene("GameLoading", LoadSceneMode.Single);
		});
	}

	public void ContinueExistingGame(SavegameReference entry)
	{
		FadeOut()?.OnComplete(delegate
		{
			SavegameBlobReader savegameReader = new SavegameSerializer().Read(entry.FullPath);
			Globals.CurrentGameStartOptionsPassOver = new GameStartOptionsContinueExisting
			{
				SavegameReader = savegameReader,
				MenuMode = false,
				UID = entry.UID
			};
			DOTween.KillAll();
			Singleton<GameCore>.G.Dispose();
			SceneManager.LoadScene("GameLoading", LoadSceneMode.Single);
		});
	}

	public void ExitGame()
	{
		FadeOut()?.OnComplete(Application.Quit);
	}
}
