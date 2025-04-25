using TMPro;
using Unity.Core.View;
using Unity.Mathematics;
using UnityEngine;

public class HUDSystemInfoText : HUDComponent, IRunnableView, IView
{
	[SerializeField]
	private TMP_Text UITextFPS;

	[SerializeField]
	private TMP_Text UITextGPUInfo;

	[SerializeField]
	private TMP_Text UITextCPUInfo;

	private float LastFPSUpdate = 0f;

	public void Run()
	{
		UITextGPUInfo.text = SystemInfo.graphicsDeviceName + " - " + SystemInfo.graphicsDeviceType.ToString() + " - " + SystemInfo.graphicsDeviceVendor + " - " + SystemInfo.graphicsDeviceVersion + " - " + StringFormatting.FormatIntegerRaw(SystemInfo.graphicsMemorySize) + " MB VRAM";
		UITextCPUInfo.text = SystemInfo.deviceModel + " - " + SystemInfo.processorType + " - " + SystemInfo.processorCount + " x " + SystemInfo.processorFrequency + " MHz - " + StringFormatting.FormatIntegerRaw(SystemInfo.systemMemorySize) + " MB RAM";
	}

	protected override void OnDispose()
	{
	}

	protected override void OnUpdate(InputDownstreamContext context)
	{
		if (Time.time - LastFPSUpdate > 0.5f)
		{
			LastFPSUpdate = Time.time;
			UITextFPS.text = math.round(1f / Time.smoothDeltaTime) + " FPS";
		}
	}
}
