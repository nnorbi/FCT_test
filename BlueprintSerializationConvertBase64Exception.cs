using System;

public sealed class BlueprintSerializationConvertBase64Exception : BlueprintSerializationException
{
	public BlueprintSerializationConvertBase64Exception(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
