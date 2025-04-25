using System;

public sealed class BlueprintSerializationZipException : BlueprintSerializationException
{
	public BlueprintSerializationZipException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
