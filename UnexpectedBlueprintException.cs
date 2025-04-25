using System;

public sealed class UnexpectedBlueprintException : BlueprintException
{
	public UnexpectedBlueprintException(Exception innerException)
		: base("An unexpected error occured.", innerException)
	{
	}
}
