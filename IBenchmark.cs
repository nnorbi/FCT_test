public interface IBenchmark
{
	string Title { get; }

	string Description { get; }

	int Priority => 0;

	IBenchmarkEnvironment Environment { get; }

	IBenchmarkConfiguration[] Configurations { get; }
}
