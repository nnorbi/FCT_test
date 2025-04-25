using Unity.Mathematics;
using UnityEngine;

public static class GameRenderingSetup
{
	public static void SetupCameraStack(Player player, out Camera mainCamera)
	{
		mainCamera = player.Viewport.MainCamera;
		Camera transparentCamera = player.Viewport.TransparentCamera;
		Camera[] cameras = new Camera[2] { mainCamera, transparentCamera };
		transparentCamera.transform.SetParent(mainCamera.transform);
		transparentCamera.transform.SetLocalPositionAndRotation(Vector3.zero, quaternion.identity);
		Camera[] array = cameras;
		foreach (Camera cam in array)
		{
			cam.orthographic = false;
			cam.nearClipPlane = 0.4f;
			cam.farClipPlane = 1000000f;
			cam.fieldOfView = 45f;
		}
	}
}
