using System;

public sealed class BlueprintSerializationParsingException : BlueprintSerializationException, IEquatable<BlueprintSerializationParsingException>
{
	public string TokenName { get; }

	public string TokenValue { get; }

	public BlueprintSerializationParsingException(string tokenName, string tokenValue)
	{
		TokenName = tokenName;
		TokenValue = tokenValue;
	}

	public bool Equals(BlueprintSerializationParsingException other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return TokenName == other.TokenName && TokenValue == other.TokenValue;
	}

	public override bool Equals(object obj)
	{
		return this == obj || (obj is BlueprintSerializationParsingException other && Equals(other));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(TokenName, TokenValue);
	}
}
