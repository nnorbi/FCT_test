using System;

public class HUDSidePanelHotkeyInfoData
{
	public string TitleId = "";

	public string DescriptionId = "";

	public string IconId = "";

	public string KeybindingId = "";

	public Action Handler;

	public Func<bool> ActiveIf;

	public bool KeybindingIsToggle = false;

	public bool DoNotListenForKeybinding = false;

	public string AdditionalKeybindingId = null;
}
