using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HUDDialogChangeKeybinding : HUDDialog
{
	private static HashSet<KeyCode> MODIFIER_CODES = new HashSet<KeyCode>
	{
		KeyCode.LeftAlt,
		KeyCode.LeftShift,
		KeyCode.LeftMeta,
		KeyCode.LeftMeta,
		KeyCode.LeftControl,
		KeyCode.LeftMeta,
		KeyCode.RightAlt,
		KeyCode.RightShift,
		KeyCode.RightMeta,
		KeyCode.RightMeta,
		KeyCode.RightControl,
		KeyCode.RightMeta
	};

	private static HashSet<KeyCode> IGNORED_CODES = new HashSet<KeyCode>
	{
		KeyCode.LeftWindows,
		KeyCode.RightWindows
	};

	public TMP_Text UIDescription;

	public TMP_Text UIBindingDisplayText;

	public Button UIBtnClear;

	public Button UIBtnAssign;

	[NonSerialized]
	public UnityEvent OnChanged = new UnityEvent();

	protected KeySet PendingSet = KeySet.EMPTY;

	protected Keybinding CurrentBinding = null;

	protected int CurrentSetIndex = 0;

	public void InitDialogContents(Keybinding binding, int index)
	{
		PendingSet = binding.GetKeySetAt(index);
		UIDescription.text = "edit-keybindings.description".tr().Replace("<name>", binding.Title);
		UIBindingDisplayText.text = KeyCodeFormatter.Resolve(PendingSet);
		HUDTheme.PrepareTheme(UIBtnClear, HUDTheme.ButtonColorsSecondary).onClick.AddListener(HandleClear);
		HUDTheme.PrepareTheme(UIBtnAssign, HUDTheme.ButtonColorsActive).onClick.AddListener(HandleConfirm);
		if (PendingSet.Empty)
		{
			UIBindingDisplayText.text = "…";
		}
		CurrentBinding = binding;
		CurrentSetIndex = index;
	}

	protected override void HandleConfirm()
	{
		CurrentBinding.OverrideKeySet(CurrentSetIndex, PendingSet);
		CurrentBinding.Save();
		OnChanged.Invoke();
		CloseRequested.Invoke();
	}

	protected void HandleClear()
	{
		CurrentBinding.OverrideKeySet(CurrentSetIndex, KeySet.EMPTY);
		CurrentBinding.Save();
		OnChanged.Invoke();
		CloseRequested.Invoke();
	}

	public override void OnGameUpdate(InputDownstreamContext context)
	{
		if (base.Visible)
		{
			if (PendingSet.Equals(KeySet.EMPTY))
			{
				float alpha = HUDTheme.PulseAnimation();
				UIBindingDisplayText.color = UIBindingDisplayText.color.WithAlpha(alpha);
			}
			else
			{
				UIBindingDisplayText.color = UIBindingDisplayText.color.WithAlpha();
			}
			context.ConsumeAll();
			bool mouseOverButton = context.UIHoverElement?.GetComponent<Button>() != null;
			KeyCode pressed = ListenToKeyPressed();
			if (pressed != KeyCode.None && !(pressed == KeyCode.Mouse0 && mouseOverButton))
			{
				GetModifiers(pressed, out var modifier0, out var modifier1);
				PendingSet = new KeySet(pressed, modifier0, modifier1);
				UIBindingDisplayText.text = KeyCodeFormatter.Resolve(PendingSet);
			}
		}
	}

	protected static KeyCode ListenToKeyPressed()
	{
		int maxCode = 330;
		for (int i = 0; i < maxCode; i++)
		{
			KeyCode code = (KeyCode)i;
			if (!IGNORED_CODES.Contains(code) && Input.GetKeyDown(code))
			{
				return code;
			}
		}
		return KeyCode.None;
	}

	protected static void GetModifiers(KeyCode current, out KeyCode modifier1, out KeyCode modifier2)
	{
		modifier1 = MODIFIER_CODES.FirstOrDefault((KeyCode code) => code != current && Input.GetKey(code));
		if (modifier1 == KeyCode.None)
		{
			modifier2 = KeyCode.None;
			return;
		}
		KeyCode modifierCopy = modifier1;
		modifier2 = MODIFIER_CODES.FirstOrDefault((KeyCode keyCode) => keyCode != current && keyCode != modifierCopy && Input.GetKey(keyCode));
	}
}
