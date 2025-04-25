using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;

public static class BenchmarkCsvExporter
{
	public static void Export(string path, BenchmarkResults results)
	{
		File.WriteAllText(path, CreateLastSessionBenchmarkCsv(results));
	}

	private static string CreateLastSessionBenchmarkCsv(BenchmarkResults benchmarkResults)
	{
		int maxFrames = benchmarkResults.StreamMap.Values.Max((FramesStream x) => x.FrameCount);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Configuration, Avg, 1% Low");
		for (int i = 0; i < maxFrames; i++)
		{
			stringBuilder.Append($", {i}");
		}
		stringBuilder.AppendLine("");
		foreach (var (config, stream) in benchmarkResults.StreamMap)
		{
			stream.CalculateMetrics(out var avg, out var onePercent, out var dotOnePercent);
			stringBuilder.Append($"{benchmarkResults.Title} ({config.Title}), {avg}, {onePercent}, {dotOnePercent}");
			using NativeArray<int> frames = stream.FramesPerSecond(Allocator.Temp);
			for (int i2 = 0; i2 < frames.Length; i2++)
			{
				stringBuilder.Append($", {frames[i2]}");
			}
			stringBuilder.AppendLine("");
		}
		return stringBuilder.ToString();
	}
}
