using System;

public sealed class BlueprintSerializationJsonException : BlueprintSerializationException
{
	public BlueprintSerializationJsonException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
