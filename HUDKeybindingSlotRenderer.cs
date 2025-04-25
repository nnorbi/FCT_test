using System;
using Core.Dependency;
using UnityEngine;
using UnityEngine.UI;

public class HUDKeybindingSlotRenderer : HUDComponent
{
	[SerializeField]
	private HUDKeySetRenderer UIKeySetRenderer;

	[SerializeField]
	private Button UIMainButton;

	[SerializeField]
	private Button UIResetButton;

	[SerializeField]
	private GameObject UIModifiedIndicator;

	private Keybinding _Keybinding;

	private int _Slot = -1;

	private IHUDDialogStack DialogStack;

	public Keybinding Keybinding
	{
		set
		{
			if (_Keybinding != null)
			{
				throw new InvalidOperationException("Can not change keybinding afterwards.");
			}
			if (_Keybinding != value)
			{
				_Keybinding = value;
				_Keybinding.Changed.AddListener(OnKeybindingChanged);
				Render();
			}
		}
	}

	public int Slot
	{
		set
		{
			if (_Slot != value)
			{
				_Slot = value;
				Render();
			}
		}
	}

	private void Render()
	{
		if (_Slot >= 0 && _Keybinding != null)
		{
			KeySet keySet = _Keybinding.GetKeySetAt(_Slot);
			UIKeySetRenderer.CurrentSet = keySet;
			UIModifiedIndicator.SetActiveSelfExt(_Keybinding.IsKeySetOverridden(_Slot));
		}
	}

	[Construct]
	private void Construct(IHUDDialogStack dialogStack)
	{
		DialogStack = dialogStack;
		AddChildView(UIKeySetRenderer);
		UIMainButton.onClick.AddListener(OnMainButtonClicked);
		UIResetButton.onClick.AddListener(OnResetButtonClicked);
	}

	private void OnMainButtonClicked()
	{
		if (_Slot >= 0 && _Keybinding != null)
		{
			HUDTheme.AnimateElementInteracted(UIMainButton.transform);
			HUDDialogChangeKeybinding dialog = DialogStack.ShowUIDialog<HUDDialogChangeKeybinding>();
			dialog.InitDialogContents(_Keybinding, _Slot);
		}
	}

	private void OnKeybindingChanged()
	{
		Render();
	}

	private void OnResetButtonClicked()
	{
		HUDTheme.AnimateElementInteracted(UIResetButton.transform);
		_Keybinding.ResetKeySet(_Slot);
		HUDTheme.AnimateElementInteracted(UIMainButton.transform);
	}

	protected override void OnDispose()
	{
		UIMainButton.onClick.RemoveListener(OnMainButtonClicked);
		UIResetButton.onClick.RemoveListener(OnResetButtonClicked);
		_Keybinding?.Changed.RemoveListener(OnKeybindingChanged);
	}
}
