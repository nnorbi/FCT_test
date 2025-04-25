public sealed class BlueprintSerializationSavegameVersionException : BlueprintSerializationVersionException
{
	public BlueprintSerializationSavegameVersionException(int serializedVersion, int minimumSupportedVersion, int maximumSupportedVersion)
		: base(serializedVersion, minimumSupportedVersion, maximumSupportedVersion)
	{
	}
}
