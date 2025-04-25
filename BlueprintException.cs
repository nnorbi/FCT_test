using System;

public abstract class BlueprintException : Exception
{
	protected BlueprintException()
	{
	}

	protected BlueprintException(string message)
		: base(message)
	{
	}

	protected BlueprintException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
