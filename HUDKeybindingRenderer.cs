using System;
using Core.Dependency;
using TMPro;
using UnityEngine;

public class HUDKeybindingRenderer : HUDComponent
{
	[SerializeField]
	private TMP_Text UIKeybindingTitle;

	[SerializeField]
	private HUDKeybindingSlotRenderer UISlot0Renderer;

	[SerializeField]
	private HUDKeybindingSlotRenderer UISlot1Renderer;

	private Keybinding _Keybinding;

	public Keybinding Keybinding
	{
		set
		{
			if (_Keybinding != null)
			{
				throw new InvalidOperationException("Can not change keybinding afterwards.");
			}
			_Keybinding = value;
			RenderKeybinding();
		}
	}

	private void RenderKeybinding()
	{
		UIKeybindingTitle.text = _Keybinding.Title;
		UISlot0Renderer.Keybinding = _Keybinding;
		UISlot1Renderer.Keybinding = _Keybinding;
	}

	[Construct]
	private void Construct()
	{
		AddChildView(UISlot0Renderer);
		AddChildView(UISlot1Renderer);
		UISlot0Renderer.Slot = 0;
		UISlot1Renderer.Slot = 1;
	}

	protected override void OnDispose()
	{
	}
}
