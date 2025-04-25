public struct ModMetadata
{
	public string Name { get; }

	public string Creator { get; }

	public string Version { get; }

	public ModMetadata(string name, string creator, string version)
	{
		Name = name;
		Creator = creator;
		Version = version;
	}
}
