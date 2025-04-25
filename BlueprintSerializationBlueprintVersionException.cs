public sealed class BlueprintSerializationBlueprintVersionException : BlueprintSerializationVersionException
{
	public BlueprintSerializationBlueprintVersionException(int serializedVersion, int minimumSupportedVersion, int maximumSupportedVersion)
		: base(serializedVersion, minimumSupportedVersion, maximumSupportedVersion)
	{
	}
}
