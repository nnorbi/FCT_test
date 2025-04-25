using System;
using Unity.Collections;
using UnityEngine;

public struct FramesStream : IDisposable
{
	private NativeList<float> FramesTime;

	public int FrameCount => FramesTime.Length;

	public void RecordLastFrame()
	{
		FramesTime.Add(Time.unscaledDeltaTime);
	}

	public void CalculateMetrics(out int avg, out int onePercent, out int dotOnePercent)
	{
		NativeArray<float> orderedMilli = new NativeArray<float>(FramesTime.Length, Allocator.Temp);
		orderedMilli.CopyFrom(FramesTime.AsArray());
		orderedMilli.Sort();
		float totalAvgFps = 0f;
		float totalOnePercent = 0f;
		float totalDotOnePercent = 0f;
		int onePercentLength = Mathf.CeilToInt((float)FramesTime.Length * 0.01f);
		int dotOnePercentLength = Mathf.CeilToInt((float)FramesTime.Length * 0.001f);
		for (int i = 0; i < FramesTime.Length; i++)
		{
			totalAvgFps += 1f / FramesTime[i];
		}
		for (int j = 0; j < onePercentLength; j++)
		{
			float num = totalOnePercent;
			int num2 = 1 + j;
			totalOnePercent = num + 1f / orderedMilli[orderedMilli.Length - num2];
		}
		for (int k = 0; k < dotOnePercentLength; k++)
		{
			float num3 = totalDotOnePercent;
			int num2 = 1 + k;
			totalDotOnePercent = num3 + 1f / orderedMilli[orderedMilli.Length - num2];
		}
		avg = Mathf.RoundToInt(totalAvgFps / (float)FramesTime.Length);
		onePercent = Mathf.RoundToInt(totalOnePercent / (float)onePercentLength);
		dotOnePercent = Mathf.RoundToInt(totalDotOnePercent / (float)dotOnePercentLength);
	}

	public NativeArray<int> FramesPerSecond(Allocator allocator)
	{
		NativeArray<int> frames = new NativeArray<int>(FramesTime.Length, allocator);
		for (int i = 0; i < FramesTime.Length; i++)
		{
			frames[i] = Mathf.RoundToInt(1f / FramesTime[i]);
		}
		FramesTime.Add(Time.unscaledDeltaTime);
		return frames;
	}

	public static FramesStream CreateNew(Allocator allocator, float benchmarkTime = 8f)
	{
		int expectedFrames = (int)(benchmarkTime * 60f);
		return new FramesStream
		{
			FramesTime = new NativeList<float>(expectedFrames, allocator)
		};
	}

	public void Dispose()
	{
		FramesTime.Dispose();
	}
}
