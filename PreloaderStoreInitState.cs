using System;
using UnityEngine;

public class PreloaderStoreInitState : PreloaderState
{
	public override void OnEnterState()
	{
		IStorePlatformSDK sdk = CreateSDK();
		Debug.Log("Initializing store SDK for platform " + GameEnvironmentManager.STORE.ToString() + " -> " + sdk.GetType().Name);
		try
		{
			sdk.Init();
		}
		catch (Exception ex)
		{
			PreloaderController.StopLoadingWithMessage("Failed to initialize " + GameEnvironmentManager.STORE.ToString() + " SDK: " + ex.Message);
			return;
		}
		Debug.Log("Initialized store SDK for platform " + GameEnvironmentManager.STORE);
		PreloaderController.MoveToNextState();
	}

	private IStorePlatformSDK CreateSDK()
	{
		if (GameEnvironmentManager.FLAG_DISABLE_STORE_SDK)
		{
			Debug.Log("Forcibly disabling store SDK");
			return new NoSdkStorePlatformSDK();
		}
		return new NoSdkStorePlatformSDK();
	}

	protected override void OnDispose()
	{
	}
}
