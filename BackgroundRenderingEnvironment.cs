using System;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class BackgroundRenderingEnvironment : IBenchmarkEnvironment
{
	private Action OnBenchmarkComplete;

	private Transform CameraControllerParent;

	private float StartTime;

	public GameStartOptions GetGameStartOptions()
	{
		return BenchmarkUtils.NewGame();
	}

	public async void OnLevelLoad(Action onBenchmarkStart, Action onBenchmarkComplete)
	{
		Transform mainCameraTransform = Singleton<GameCore>.G.DrawSceneReferences.MainCamera.transform;
		Transform cameraControllerTransform = mainCameraTransform.parent;
		cameraControllerTransform.transform.position = Vector3.zero;
		cameraControllerTransform.position = new float3(10f, 0f, -50f);
		cameraControllerTransform.rotation = Quaternion.identity;
		mainCameraTransform.localPosition = new float3(0f, 20f, -40f);
		mainCameraTransform.localRotation = Quaternion.Euler(30f, 0f, 0f);
		CameraControllerParent = cameraControllerTransform;
		OnBenchmarkComplete = onBenchmarkComplete;
		StartTime = Time.time;
		GameRenderingSetup.SetupCameraStack(Singleton<GameCore>.G.LocalPlayer, out var _);
		await Task.Delay(1000);
		GameCoreHooks hooks = Singleton<GameCore>.G.Hooks;
		hooks.AfterInputUpdate = (Action)Delegate.Combine(hooks.AfterInputUpdate, new Action(OnUpdate));
		onBenchmarkStart();
	}

	private void OnUpdate()
	{
		float elapsed = Time.time - StartTime;
		if (elapsed < 6f)
		{
			float t = elapsed / 6f;
			CameraControllerParent.rotation = FastMatrix.RotateYAngle(t * 360f);
		}
		else
		{
			OnBenchmarkComplete();
			GameCoreHooks hooks = Singleton<GameCore>.G.Hooks;
			hooks.AfterInputUpdate = (Action)Delegate.Remove(hooks.AfterInputUpdate, new Action(OnUpdate));
		}
	}
}
