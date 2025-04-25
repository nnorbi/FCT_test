public interface IMod
{
	ModMetadata Metadata { get; }

	void Init(string path);
}
