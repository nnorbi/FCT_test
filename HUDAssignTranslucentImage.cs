using System;
using LeTai.Asset.TranslucentImage;
using UnityEngine;

public class HUDAssignTranslucentImage : MonoBehaviour
{
	public delegate void OnGameCameraCreatedDelegate(Camera cam);

	public static OnGameCameraCreatedDelegate OnGameCameraCreated = delegate
	{
	};

	protected TranslucentImage Image;

	public void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Image = GetComponent<TranslucentImage>();
		OnGameCameraCreated = (OnGameCameraCreatedDelegate)Delegate.Combine(OnGameCameraCreated, new OnGameCameraCreatedDelegate(OnCameraCreated));
		if (Image == null)
		{
			Debug.LogError("MonoBehaviour contains no TranslucentImage: " + base.name);
		}
		if (Singleton<GameCore>.HasInstance && Singleton<GameCore>.G.Initialized)
		{
			Camera cam = Singleton<GameCore>.G.LocalPlayer.Viewport.MainCamera;
			TranslucentImageSource source = cam.GetComponent<TranslucentImageSource>();
			if (source != null)
			{
				Image.source = source;
				Image.material = Globals.Resources.TranslucentDefaultMaterial;
			}
			else
			{
				Debug.LogError("Main cam has no transparent image source");
			}
		}
	}

	public void OnDestroy()
	{
		OnGameCameraCreated = (OnGameCameraCreatedDelegate)Delegate.Remove(OnGameCameraCreated, new OnGameCameraCreatedDelegate(OnCameraCreated));
	}

	protected void OnCameraCreated(Camera cam)
	{
		if (Application.isPlaying && Image != null)
		{
			Image.source = cam.GetComponent<TranslucentImageSource>();
			Image.material = Globals.Resources.TranslucentDefaultMaterial;
			if (Image.source == null)
			{
				Debug.LogError("Main cam transparent image source component #2");
			}
		}
	}
}
