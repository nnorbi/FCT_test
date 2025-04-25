using System;

public abstract class BlueprintSerializationException : BlueprintException
{
	protected BlueprintSerializationException()
	{
	}

	protected BlueprintSerializationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
