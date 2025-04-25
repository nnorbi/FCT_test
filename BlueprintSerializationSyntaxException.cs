using System;

public sealed class BlueprintSerializationSyntaxException : BlueprintSerializationException, IEquatable<BlueprintSerializationSyntaxException>
{
	public string MissingToken { get; }

	public BlueprintSerializationSyntaxException(string missingToken)
	{
		MissingToken = missingToken;
	}

	public bool Equals(BlueprintSerializationSyntaxException other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return MissingToken == other.MissingToken;
	}

	public override bool Equals(object obj)
	{
		return this == obj || (obj is BlueprintSerializationSyntaxException other && Equals(other));
	}

	public override int GetHashCode()
	{
		return (MissingToken != null) ? MissingToken.GetHashCode() : 0;
	}
}
