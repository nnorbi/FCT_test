using System;

public abstract class BlueprintSerializationVersionException : BlueprintSerializationException, IEquatable<BlueprintSerializationVersionException>
{
	public int SerializedVersion { get; }

	public int MinimumSupportedVersion { get; }

	public int MaximumSupportedVersion { get; }

	protected BlueprintSerializationVersionException(int serializedVersion, int minimumSupportedVersion, int maximumSupportedVersion)
	{
		SerializedVersion = serializedVersion;
		MinimumSupportedVersion = minimumSupportedVersion;
		MaximumSupportedVersion = maximumSupportedVersion;
	}

	public bool Equals(BlueprintSerializationVersionException other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		return SerializedVersion == other.SerializedVersion && MinimumSupportedVersion == other.MinimumSupportedVersion && MaximumSupportedVersion == other.MaximumSupportedVersion;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((BlueprintSerializationVersionException)obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(SerializedVersion, MinimumSupportedVersion, MaximumSupportedVersion);
	}
}
