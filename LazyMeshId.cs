using System;

public readonly struct LazyMeshId : IEquatable<LazyMeshId>
{
	private readonly int Id;

	public LazyMeshId(int id)
	{
		Id = id;
	}

	public bool Equals(LazyMeshId other)
	{
		return Id == other.Id;
	}

	public override bool Equals(object obj)
	{
		return obj is LazyMeshId other && Equals(other);
	}

	public override int GetHashCode()
	{
		return Id;
	}
}
