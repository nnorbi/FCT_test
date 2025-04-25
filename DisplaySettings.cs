using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class DisplaySettings : GameSettingsGroup
{
	public EnumGameSetting<DisplayFullScreenMode> WindowMode = new EnumGameSetting<DisplayFullScreenMode>("window-mode", DisplayFullScreenMode.FullScreen);

	public ResolutionGameSetting Resolution = new ResolutionGameSetting("resolution");

	public EnumGameSetting<DisplayVSyncMode> VSync = new EnumGameSetting<DisplayVSyncMode>("vsync", DisplayVSyncMode.Full);

	private static DisplayResolution FindClosestSupportedFullScreenResolution(DisplayResolution desiredResolution)
	{
		return (from r in Screen.resolutions
			select new DisplayResolution(r) into r
			orderby math.distancesq(r.Dimensions, desiredResolution.Dimensions)
			select r).First();
	}

	public DisplaySettings(bool saveOnChange)
		: base("display", saveOnChange)
	{
		Register(WindowMode);
		Register(Resolution);
		Register(VSync);
	}

	public void InitDisplay()
	{
		DisplayResolution resolution = Resolution.Value;
		Debug.Log($"DisplaySettings::Prepare resolution, stored is {resolution} / {WindowMode.Value}");
		if (!resolution.Equals(DisplayResolution.UNINITIALIZED))
		{
			Apply();
			return;
		}
		if (WindowMode.Value == DisplayFullScreenMode.Windowed)
		{
			Resolution.SetValue(new DisplayResolution(1280, 720));
			Debug.LogWarning("DisplaySettings:: Reset window size to basic size");
		}
		else
		{
			Resolution current = Screen.currentResolution;
			Resolution.SetValue(new DisplayResolution(current));
			Resolution resolution2 = current;
			Debug.Log("DisplaySettings:: Init screen resolution to current: " + resolution2.ToString());
		}
		Apply();
	}

	public void Apply()
	{
		if (Resolution.Value.Equals(DisplayResolution.UNINITIALIZED))
		{
			Debug.LogError("DisplaySettings:: Invalid resolution in settings, ignoring.");
		}
		else if (WindowMode.Value == DisplayFullScreenMode.Windowed)
		{
			Debug.Log($"DisplaySettings:: Screen.SetResolution {Resolution.Value} WINDOWED");
			Screen.SetResolution(Resolution.Value.Width, Resolution.Value.Height, FullScreenMode.Windowed);
		}
		else
		{
			DisplayResolution targetResolution = FindClosestSupportedFullScreenResolution(Resolution.Value);
			Debug.Log($"DisplaySettings:: Screen.SetResolution {targetResolution} (From {Resolution.Value}) {WindowMode.Value}");
			Screen.SetResolution(targetResolution.Width, targetResolution.Height, (FullScreenMode)WindowMode.Value);
		}
		if (VSync.Value == DisplayVSyncMode.Off)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 300;
		}
		else
		{
			QualitySettings.vSyncCount = (int)VSync.Value;
			Application.targetFrameRate = -1;
		}
		Screen.sleepTimeout = -1;
		QualitySettings.maxQueuedFrames = 0;
	}
}
