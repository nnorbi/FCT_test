using System.Collections.Generic;
using System.Text;

public class BenchmarkResults
{
	public string Title;

	public Dictionary<IBenchmarkConfiguration, FramesStream> StreamMap = new Dictionary<IBenchmarkConfiguration, FramesStream>();

	public BenchmarkResults(string title)
	{
		Title = title;
	}

	public void Dispose()
	{
		foreach (KeyValuePair<IBenchmarkConfiguration, FramesStream> item in StreamMap)
		{
			item.Value.Dispose();
		}
	}

	public override string ToString()
	{
		StringBuilder text = new StringBuilder();
		foreach (var (configuration, stream) in StreamMap)
		{
			stream.CalculateMetrics(out var avg, out var onePercent, out var dotOnePercent);
			text.AppendLine(configuration.Title ?? "");
			text.AppendLine($"\tAVG: {avg} FPS");
			text.AppendLine($"\t1%: {onePercent} FPS");
			text.AppendLine($"\t0.1%: {dotOnePercent} FPS");
		}
		return text.ToString();
	}
}
