using System.Collections.Generic;
using Core.Collections.Scoped;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameInputManager
{
	protected int UILayer;

	protected readonly Dictionary<Keybinding, (Keybinding, KeySet)> LastBindings = new Dictionary<Keybinding, (Keybinding, KeySet)>();

	protected float2 LastMousePos = new float2(0f, 0f);

	private float LastMouseInput = -1E+10f;

	private float LastControllerInput = -1E+10f;

	public IInputSourceProvider InputSource;

	private Keybindings Keybindings;

	public InputDownstreamContext DownstreamContext { get; }

	private static float DetermineWheelDelta()
	{
		float wheelDelta = Input.mouseScrollDelta.y;
		if (Input.GetKey(KeyCode.LeftShift))
		{
			wheelDelta = Input.mouseScrollDelta.x;
		}
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
		{
			wheelDelta *= -4f;
		}
		return math.clamp(wheelDelta, -15f, 15f);
	}

	public GameInputManager(IInputSourceProvider inputSource, Keybindings keybindings)
	{
		InputSource = inputSource;
		Keybindings = keybindings;
		UILayer = LayerMask.NameToLayer("UI");
		DownstreamContext = new InputDownstreamContext(Keybindings.KeybindingsById);
		Cursor.visible = true;
	}

	public void OnGameUpdate()
	{
		using ScopedList<(Keybinding, KeySet)> activeBindings = ScopedList<(Keybinding, KeySet)>.Get();
		using ScopedList<(Keybinding, KeySet)> allActiveBindings = ScopedList<(Keybinding, KeySet)>.Get();
		Vector3 mousePosRaw = Input.mousePosition;
		float2 mousePos = new float2(mousePosRaw.x, mousePosRaw.y);
		float2 realMouseDelta = mousePos - LastMousePos;
		LastMousePos = mousePos;
		bool receiveInputs = Application.isFocused && mousePos.x >= 0f && mousePos.y >= 0f && mousePos.x <= (float)Screen.width && mousePos.y <= (float)Screen.height;
		float wheelDelta = (receiveInputs ? DetermineWheelDelta() : 0f);
		GameObject uiHoverElement = (receiveInputs ? GetUIHoverElement() : null);
		if (receiveInputs)
		{
			foreach (KeybindingsLayer layer in Keybindings.Layers)
			{
				Dictionary<KeyCode, (Keybinding, KeySet)> bestBindingPerCode = new Dictionary<KeyCode, (Keybinding, KeySet)>();
				foreach (Keybinding binding in layer.Bindings)
				{
					for (int setIndex = 0; setIndex < 2; setIndex++)
					{
						KeySet keySet = binding.GetKeySetAt(setIndex);
						if ((!binding.GenerallyBlockableByUI || !keySet.IsBlockableByUI() || !(uiHoverElement != null) || LastBindings.ContainsKey(binding)) && keySet.IsCurrentlyActive(binding.AxisThreshold))
						{
							allActiveBindings.Add((binding, keySet));
							(Keybinding, KeySet) currentBest;
							if (keySet.ControllerSource != ControllerBinding.None)
							{
								activeBindings.Add((binding, keySet));
							}
							else if (!bestBindingPerCode.TryGetValue(keySet.Code, out currentBest) || currentBest.Item2.GetPriority() < keySet.GetPriority())
							{
								bestBindingPerCode[keySet.Code] = (binding, keySet);
							}
							break;
						}
					}
				}
				foreach (var keySetByBinding in bestBindingPerCode.Values)
				{
					activeBindings.Add(keySetByBinding);
				}
			}
		}
		if (math.abs(math.length(realMouseDelta)) > 0.01f)
		{
			LastMouseInput = Time.unscaledTime;
		}
		bool controllerActive = LastControllerInput > LastMouseInput;
		GameInputModeType inputMode = (controllerActive ? GameInputModeType.Controller : GameInputModeType.KeyboardMouse);
		if (inputMode != InputSource.InputMode)
		{
			InputSource.ChangeInputMode(inputMode);
			Cursor.visible = !controllerActive;
		}
		if (InputSource.InputMode != GameInputModeType.Controller)
		{
			GameObject currentSelection = EventSystem.current.currentSelectedGameObject;
			if (currentSelection != null && !currentSelection.GetComponent<TMP_InputField>())
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}
		Shader.SetGlobalVector(GlobalShaderInputs.MousePosition, new Vector2(mousePosRaw.x, mousePosRaw.y));
		DownstreamContext.Update(LastBindings.Values, activeBindings, mousePos, realMouseDelta, wheelDelta, uiHoverElement);
		LastBindings.Clear();
		foreach (var (binding2, key) in allActiveBindings)
		{
			LastBindings.Add(binding2, (binding2, key));
		}
	}

	protected GameObject GetUIHoverElement()
	{
		if (EventSystem.current == null)
		{
			return null;
		}
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = ((float3)Input.mousePosition).xy;
		List<RaycastResult> raycastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, raycastResults);
		foreach (RaycastResult result in raycastResults)
		{
			if (result.gameObject == null || result.gameObject.layer != UILayer || !result.gameObject.activeInHierarchy || result.gameObject.GetComponent<TMP_Text>() != null)
			{
				continue;
			}
			Button btn = result.gameObject.GetComponent<Button>();
			if (btn != null && !btn.interactable)
			{
				return null;
			}
			return result.gameObject;
		}
		return null;
	}
}
