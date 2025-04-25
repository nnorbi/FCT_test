using System;
using UnityEngine;

[Serializable]
public class HUDMainButtonConfig
{
	public Sprite Icon;

	public HUDPart Part;

	public HUDMainButtons.ButtonLocation Location;

	public string TooltipHeaderId = "";

	public string KeybindingId = "";

	public bool ListenToKeybinding = true;

	public Func<bool> IsVisible;

	public Func<bool> IsEnabled;

	public Func<bool> IsActive;

	public Func<bool> HasBadge;

	public Action OnActivate;
}
