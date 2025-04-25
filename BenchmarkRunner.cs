#define UNITY_ASSERTIONS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using Tayx.Graphy;
using Tayx.Graphy.Utils;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BenchmarkRunner
{
	private readonly IBenchmark Benchmark;

	private readonly Action<BenchmarkResults> OnComplete;

	private BenchmarkResults Results;

	private int OriginSceneId;

	public BenchmarkRunner(IBenchmark benchmark, Action<BenchmarkResults> onComplete)
	{
		Benchmark = benchmark;
		OnComplete = onComplete;
	}

	public void Run()
	{
		Debug.Assert(Benchmark.Configurations.Length != 0);
		GraphicSettings originalSettings = new GraphicSettings(saveOnChange: false);
		originalSettings.Load();
		Queue<IBenchmarkConfiguration> queue = new Queue<IBenchmarkConfiguration>(Benchmark.Configurations);
		Results = new BenchmarkResults(Benchmark.Title);
		OriginSceneId = SceneManager.GetActiveScene().buildIndex;
		RunNextConfiguration();
		void RunNextConfiguration()
		{
			if (queue.Count == 0)
			{
				Globals.Settings.Graphics.CopyFrom(originalSettings);
				CompleteBenchmark();
			}
			else
			{
				RunBenchmarkConfiguration(Benchmark.Environment, queue.Dequeue(), RunNextConfiguration);
			}
		}
	}

	private void RunBenchmarkConfiguration(IBenchmarkEnvironment environment, IBenchmarkConfiguration configuration, Action onBenchmarkEnds)
	{
		Globals.Settings.Graphics.CopyFrom(configuration.RequiredGraphicSettings());
		Globals.CurrentGameStartOptionsPassOver = environment.GetGameStartOptions();
		DOTween.KillAll();
		SceneManager.LoadScene("GameLoading", LoadSceneMode.Single);
		SceneManager.sceneLoaded += OnSceneLoaded;
		void OnGameLoad()
		{
			new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
			bool benchmarkingRunning = true;
			environment.OnLevelLoad(OnBenchmarkStarts, OnBenchmarkEnds);
			void OnBenchmarkEnds()
			{
				G_Singleton<GraphyManager>.Instance.Disable();
				benchmarkingRunning = false;
				onBenchmarkEnds();
			}
			async void OnBenchmarkStarts()
			{
				G_Singleton<GraphyManager>.Instance.Enable();
				if (!Results.StreamMap.TryGetValue(configuration, out var stream))
				{
					stream = FramesStream.CreateNew(Allocator.Persistent);
					Results.StreamMap.Add(configuration, stream);
				}
				while (Application.isPlaying && benchmarkingRunning)
				{
					stream.RecordLastFrame();
					await Task.Yield();
				}
			}
		}
		void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (!(scene.name != "Ingame"))
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				Singleton<GameCore>.G.OnGameInitialized.AddListener(OnGameLoad);
			}
		}
	}

	private void CompleteBenchmark()
	{
		SceneManager.LoadScene(OriginSceneId);
		SceneManager.sceneLoaded += OnSceneLoaded;
		void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			if (scene.buildIndex == OriginSceneId)
			{
				SceneManager.sceneLoaded -= OnSceneLoaded;
				OnComplete(Results);
			}
		}
	}
}
