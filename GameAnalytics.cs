using System;
using Unity.Services.Analytics;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.CrashReportHandler;

public class GameAnalytics
{
	public async void Init(bool enabled)
	{
		if (Application.isEditor)
		{
			return;
		}
		Debug.Log("Initializing analytics");
		CrashReportHandler.enableCaptureExceptions = true;
		CrashReportHandler.logBufferSize = 50u;
		_ = GameEnvironmentManager.STORE;
		GameEnvironment environment = GameEnvironmentManager.ENVIRONMENT;
		InitializationOptions options = new InitializationOptions();
		switch (environment)
		{
		case GameEnvironment.Dev:
			options.SetEnvironmentName("dev");
			break;
		case GameEnvironment.Stage:
			options.SetEnvironmentName("stage");
			break;
		case GameEnvironment.Prod:
			options.SetEnvironmentName("production");
			break;
		default:
			throw new Exception("Unknown environment: " + environment);
		}
		try
		{
			Debug.Log("Initializing analytics ...");
			await UnityServices.InitializeAsync(options);
			if (enabled)
			{
				AnalyticsService.Instance.StartDataCollection();
			}
			else
			{
				AnalyticsService.Instance.StopDataCollection();
			}
		}
		catch (Exception ex)
		{
			Debug.LogWarning("Failed to init analytics: " + ex);
			return;
		}
		Debug.Log("Analytics initialized.");
		Debug.Log("Player id:" + AuthenticationService.Instance.PlayerId);
	}
}
