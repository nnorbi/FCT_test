using System;

public sealed class BlueprintSerializationUnknownTypeException : BlueprintSerializationException, IEquatable<BlueprintSerializationUnknownTypeException>
{
	public string Type { get; }

	public BlueprintSerializationUnknownTypeException(string type)
	{
		Type = type;
	}

	public bool Equals(BlueprintSerializationUnknownTypeException other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return Type == other.Type;
	}

	public override bool Equals(object obj)
	{
		return this == obj || (obj is BlueprintSerializationUnknownTypeException other && Equals(other));
	}

	public override int GetHashCode()
	{
		return (Type != null) ? Type.GetHashCode() : 0;
	}
}
