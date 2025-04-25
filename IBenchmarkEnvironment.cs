using System;

public interface IBenchmarkEnvironment
{
	GameStartOptions GetGameStartOptions();

	void OnLevelLoad(Action onBenchmarkStart, Action onBenchmarkComplete);
}
